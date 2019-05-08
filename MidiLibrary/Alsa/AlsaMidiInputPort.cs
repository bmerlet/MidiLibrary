using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MidiLibrary.CommonMessages;
using MidiLibrary.PortIO;
using MidiLibrary.RealTimeMessages;

namespace MidiLibrary.Alsa
{
    public class AlsaMidiInputPort : IMidiInputPort
    {
        #region Port enumeration

        public static IMidiInputPort[] GetAllPorts()
        {
            var devices = AlsaUtils.EnumerateMidiPorts(AlsaNativeMethods.ESnd_rawmidi_stream.SND_RAWMIDI_STREAM_INPUT);
            var ports = new List<IMidiInputPort>();
            foreach(var dev in devices)
            {
                ports.Add(new AlsaMidiInputPort(dev.Device, dev.Name));
            }
            return ports.ToArray();
        }

        #endregion

        #region Constructor

        private AlsaMidiInputPort(string device, string name)
        {
            Device = device;
            Name = name;
        }

        #endregion

        #region private memebers

        private IntPtr handle = IntPtr.Zero;

        private bool stop = false;
        private Thread readThread = null;

        #endregion

        #region Midi input event

        public event EventHandler<IMidiEventArgs> MidiInputReceived;

        #endregion

        #region Public properties

        // Device name, e.g. hw:1,0,0
        public string Device { get; }

        // Port name, e.g. AKM320 MIDI 1
        public string Name { get; }

        #endregion

        #region Public methods

        public string Open()
        {
            // Open the port
            var st = AlsaNativeMethods.Snd_rawmidi_open_input(
                    ref handle, IntPtr.Zero, Device, AlsaNativeMethods.EMode.SND_RAWMIDI_NONBLOCK);

            return AlsaUtils.StrError(st);
        }

        public string Start()
        {
            int st = (int)AlsaNativeMethods.Snd_rawmidi_read(handle, null, 0);
            if (st == 0)
            {
                stop = false;
                readThread = new Thread(ReadThreadStart);
                readThread.Start();
            }
            return AlsaUtils.StrError(st);
        }

        public string Stop()
        {
            if (readThread != null)
            {
                stop = true;
                readThread.Join();
                readThread = null;
            }

            return null;
        }

        public string Reset()
        {
            int st = AlsaNativeMethods.Snd_rawmidi_drop(handle);
            return AlsaUtils.StrError(st);
        }

        public string Close()
        {
            int st = 0;
            if (handle != IntPtr.Zero)
            {
                st = AlsaNativeMethods.Snd_rawmidi_close(handle);
                handle = IntPtr.Zero;
            }

            return AlsaUtils.StrError(st);
        }

        #endregion

        #region Read thread

        private void ReadThreadStart()
        {
            byte[] buf = new byte[512];

            // Compute number of pollfd structures required to poll
            int numPollStructs = AlsaNativeMethods.Snd_rawmidi_poll_descriptors_count(handle);

            // Allocate space for the poll structures
            IntPtr pollAreaPtr = Marshal.AllocHGlobal(numPollStructs * AlsaNativeMethods.POLLFDSZ);

            int st = AlsaNativeMethods.Snd_rawmidi_poll_descriptors(handle, pollAreaPtr, (uint)numPollStructs);

            while (!stop)
            {
                st = AlsaNativeMethods.Poll(pollAreaPtr, numPollStructs, 200);
                if (st < 0)
                {
                    var errno = Marshal.GetLastWin32Error();
                    if (errno == 4 /* EINTR */)
                    {
                        continue;
                    }
                    Console.WriteLine("MidiIn: Cannot poll - errno = " + errno);
                    break;
                }
                if (st == 0)
                {
                    continue;
                }

                // We got something - scan the poll struct
                ushort revent = 0;
                st = AlsaNativeMethods.Snd_rawmidi_poll_descriptors_revents(
                    handle, pollAreaPtr, (uint)numPollStructs, ref revent);

                if (st < 0)
                {
                    Console.WriteLine("MidiIn: Cannot parse poll - " + AlsaUtils.StrError(st));
                    break;
                }

                AlsaNativeMethods.EPoll evt = (AlsaNativeMethods.EPoll)revent;
                if (evt.HasFlag(AlsaNativeMethods.EPoll.POLLERR) ||
                    evt.HasFlag(AlsaNativeMethods.EPoll.POLLHUP))
                {
                    break;
                }

                if (evt.HasFlag(AlsaNativeMethods.EPoll.POLLIN))
                {
                    // We have something to read
                    st = (int)AlsaNativeMethods.Snd_rawmidi_read(handle, buf, (uint)buf.Length);
                    if (st < 0)
                    {
                        Console.WriteLine("MidiIn: Cannot parse poll - " + AlsaUtils.StrError(st));
                        break;
                    }

                    // Create a midi message from the input
                    var midiMessage = ParseBuffer(buf, st);
                    var midiEvent = new MidiEvent(midiMessage, 0);
                    // ZZZ ZZZ
                    var midiEventArg = new WindowsMultiMedia.WindowsMidiEventArgs(midiEvent, IntPtr.Zero, IntPtr.Zero);
                    MidiInputReceived?.Invoke(this, midiEventArg);
                }
            }

            Marshal.FreeHGlobal(pollAreaPtr);
        }

        private MidiMessage ParseBuffer(byte[] data, int length)
        {
            MidiMessage result = null;
            uint rawCommand = data[0];
            uint channel = (rawCommand & 0x0f) + 1;
            EMidiCommand command = (EMidiCommand)(rawCommand & 0xF0);

            if (command == EMidiCommand.SystemMessageMask)
            {
                // System message
                channel = 0;
                command = (EMidiCommand)rawCommand;

                switch (command)
                {
                    case EMidiCommand.TimingClock:
                        result = new TimingClockMessage();
                        break;
                    case EMidiCommand.StartSequence:
                        result = new StartSequenceMessage();
                        break;
                    case EMidiCommand.ContinueSequence:
                        result = new ContinueSequenceMessage();
                        break;
                    case EMidiCommand.StopSequence:
                        result = new StopSequenceMessage();
                        break;
                    case EMidiCommand.ActiveSensing:
                        result = new ActiveSensingMessage();
                        break;
                }
            }
            else
            {
                uint p1 = data[1];
                uint p2 = data[2];

                // Common message
                switch (command)
                {
                    case EMidiCommand.NoteOff:
                    case EMidiCommand.NoteOn:
                    case EMidiCommand.KeyAfterTouch:
                        result = new MidiNoteMessage(channel, command, p1, p2);
                        break;
                    case EMidiCommand.ControlChange:
                        result = new MidiControlChangeMessage(channel, p1, p2);
                        break;
                    case EMidiCommand.PatchChange:
                        result = new MidiPatchChangeMessage(channel, p1);
                        break;
                    case EMidiCommand.ChannelAfterTouch:
                        result = new MidiChannelAfterTouchMessage(channel, p1);
                        break;
                    case EMidiCommand.PitchWheelChange:
                        result = new MidiPitchChangeMessage(channel, p1 + (p2 << 7));
                        break;
                }
            }

            return result;
        }

        #endregion

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
            Close();
        }

        ~AlsaMidiInputPort()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
