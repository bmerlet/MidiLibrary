using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MidiLibrary.CommonMessages;
using MidiLibrary.PortIO;
using MidiLibrary.RealTimeMessages;
using MidiLibrary.SysexMessages;

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
            byte[] overflowBuf = null;

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
                    // We have something to read - read it
                    int nRead = (int)AlsaNativeMethods.Snd_rawmidi_read(handle, buf, (uint)buf.Length);
                    if (nRead < 0)
                    {
                        int errno = Marshal.GetLastWin32Error();
                        if (errno == 4 /* EINTR */)
                        {
                            continue;
                        }

                        Console.WriteLine("MidiIn: Cannot parse poll - " + AlsaUtils.StrError(st));
                        break;
                    }

                    // Combine with overflow buffer if we had an overflow last time
                    if (overflowBuf != null)
                    {
                        var newBuf = new byte[buf.Length + overflowBuf.Length];
                        Array.Copy(overflowBuf, 0, newBuf, 0, overflowBuf.Length);
                        Array.Copy(buf, 0, newBuf, overflowBuf.Length, buf.Length);
                        buf = newBuf;
                        overflowBuf = null;
                    }

                    // Translate the input to midi message
                    int nParsed = 0;
                    while (true)
                    {
                        st = ParseBuffer(buf, nParsed, nRead, out MidiMessage midiMessage);
                        if (st < 0)
                        {
                            // buffer contains an incomplete message
                            overflowBuf = buf;
                            buf = new byte[overflowBuf.Length];
                        }
                        else if (st == 0)
                        {
                            // Message not supported - drop everything.
                            break;
                        }
                        else // st > 0
                        {
                            // Send message
                            var midiEvent = new MidiEvent(midiMessage, 0);
                            var midiEventArg = new WindowsMultiMedia.WindowsMidiEventArgs(midiEvent, IntPtr.Zero, IntPtr.Zero);
                            MidiInputReceived?.Invoke(this, midiEventArg);

                            // Update counters
                            nParsed += st;
                            nRead -= st;
                            
                            if (nRead <= 0)
                            {
                                // Normal end
                                break;
                            }
                        }
                    }
                }
            }

            Marshal.FreeHGlobal(pollAreaPtr);
        }

        private int ParseBuffer(byte[] data, int offset, int length, out MidiMessage message)
        {
            message = null;
            int messageLength = 0;
            if (length == 0)
            {
                // Should not happen
                return 0;
            }

            uint rawCommand = data[offset];
            uint channel = (rawCommand & 0x0f) + 1;
            EMidiCommand command = (EMidiCommand)(rawCommand & 0xF0);

            if (command == EMidiCommand.SystemMessageMask)
            {
                // System message
                channel = 0;
                command = (EMidiCommand)rawCommand;
                messageLength = 1;

                switch (command)
                {
                    case EMidiCommand.TimingClock:
                        message = new TimingClockMessage();
                        break;
                    case EMidiCommand.StartSequence:
                        message = new StartSequenceMessage();
                        break;
                    case EMidiCommand.ContinueSequence:
                        message = new ContinueSequenceMessage();
                        break;
                    case EMidiCommand.StopSequence:
                        message = new StopSequenceMessage();
                        break;
                    case EMidiCommand.ActiveSensing:
                        message = new ActiveSensingMessage();
                        break;
                    case EMidiCommand.SystemExclusive:
                        messageLength = ParseSysex(data, offset, length, ref message);
                        break;
                }
            }
            else
            {
                uint p1 = (length >= 2) ? data[offset + 1] : 0u;
                uint p2 = (length >= 3) ? data[offset + 2] : 0u;

                // Common message
                switch (command)
                {
                    case EMidiCommand.NoteOff:
                    case EMidiCommand.NoteOn:
                    case EMidiCommand.KeyAfterTouch:
                        message = new MidiNoteMessage(channel, command, p1, p2);
                        messageLength = 3;
                        break;
                    case EMidiCommand.ControlChange:
                        message = new MidiControlChangeMessage(channel, p1, p2);
                        messageLength = 3;
                        break;
                    case EMidiCommand.PatchChange:
                        message = new MidiPatchChangeMessage(channel, p1);
                        messageLength = 2;
                        break;
                    case EMidiCommand.ChannelAfterTouch:
                        message = new MidiChannelAfterTouchMessage(channel, p1);
                        messageLength = 2;
                        break;
                    case EMidiCommand.PitchWheelChange:
                        message = new MidiPitchChangeMessage(channel, p1 + (p2 << 7));
                        messageLength = 3;
                        break;
                }

                // Indicate truncated message in reply
                if (length < messageLength)
                {
                    messageLength = -1;

                }
            }

            return messageLength;
        }

        private int ParseSysex(byte[] data, int offset, int length, ref MidiMessage message)
        {
            var endOfSysexIndex = Array.FindIndex(data, 1, b => b == (byte)EMidiCommand.EndOfSystemExclusive);
            if (endOfSysexIndex < 0)
            {
                // Incomplete sysex
                return -1;
            }

            // Build body
            var body = new byte[endOfSysexIndex - 1];
            Array.Copy(data, 1, body, 0, body.Length);

            // Build message
            message = new MidiSysexMessage(body);

            return endOfSysexIndex + 1;
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
