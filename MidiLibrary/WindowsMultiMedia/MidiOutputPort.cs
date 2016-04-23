//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MidiLibrary.SysexMessages;
using MidiLibrary.Sequencer;

namespace MidiLibrary.WindowsMultiMedia
{
    public class MidiOutputPort : IDisposable
    {
        #region Enums

        public enum ETechnology
        {
            MidiHardwarePort = NativeMethods.ETechnology.MOD_MIDIPORT,
            Synthesizer = NativeMethods.ETechnology.MOD_SYNTH,
            SquareWaveSynthesizer = NativeMethods.ETechnology.MOD_SQSYNTH,
            FMSynthesizer = NativeMethods.ETechnology.MOD_FMSYNTH,
            MidiMapper = NativeMethods.ETechnology.MOD_MAPPER,
            WaveTableSynthesizer = NativeMethods.ETechnology.MOD_WAVETABLE,
            SoftwareSynthesizer = NativeMethods.ETechnology.MOD_SWSYNTH
        };

        #endregion

        #region Static utilities

        /// <summary>
        /// Get the number of midi output port
        /// </summary>
        public static uint Count
        {
            get { return NativeMethods.midiOutGetNumDevs(); }
        }

        /// <summary>
        /// Returns all midi output ports
        /// </summary>
        /// <returns>Array containing every midi input port defined in the system</returns>
        static public MidiOutputPort[] GetAllPorts()
        {
            uint numDevices = Count;
            MidiOutputPort[] devs = new MidiOutputPort[numDevices];

            for (uint d = 0; d < numDevices; d++)
            {
                devs[d] = new MidiOutputPort(d);
            }

            return devs;
        }

        #endregion

        #region Private members

        // Device id
        private uint id;

        // Properties of the device
        private ushort manufacturerId;
        private ushort productId;
        private string driverVersion;
        private ETechnology technology;
        private ushort maxVoices;
        private ushort maxNotes;
        private ushort channelMask;
        private string name;

        // Handle when this device is opened
        private IntPtr handle;

        // Callback function called by the native code, which in turn calls MidiProc() in this class
        private NativeMethods.MidiOutProc midiOutProc;

        // Callback provided by the user
        private MidiEventHandler callback;

        // Volume cache
        bool volumeRead;
        // Volume. Note: The low-order word of this location contains the left-channel
        // volume setting, and the high-order word contains the right-channel setting.
        // A value of 0xFFFF represents full volume, and a value of 0x0000 is silence.
        uint volume;

        #endregion

        #region Constructor & destructor

        /// <summary>
        /// Construct a new midi input port.
        /// </summary>
        /// <param name="id">Id of the port, between 0 and the number returned by InputCount</param>
        public MidiOutputPort(uint id)
        {
            // Id of this port
            this.id = id;

            // Native callback calling MidiProc() 
            midiOutProc = new NativeMethods.MidiOutProc(MidiProc);

            // Init
            handle = IntPtr.Zero;
            callback = null;
            volumeRead = false;

            // Get the capabilities of this device
            GetAndParseMidiOutCaps();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (handle != IntPtr.Zero)
            {
                NativeMethods.midiOutClose(handle);
                handle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MidiOutputPort()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public uint Id
        {
            get { return id; }
        }

        public ushort ManufacturerId
        {
            get { return manufacturerId; }
        }

        public ushort ProductId
        {
            get { return productId; }
        }

        public String Name
        {
            get { return name; }
        }

        public ETechnology Technology
        {
            get { return technology; }
        }

        public ushort MaxVoices
        {
            get { return maxVoices; }
        }

        public ushort MaxNotes
        {
            get { return maxNotes; }
        }

        public ushort ChannelMask
        {
            get { return channelMask; }
        }

        public String DriverVersion
        {
            get { return driverVersion; }
        }

        public uint Volume
        {
            get
            {
                if (!volumeRead)
                {
                    NativeMethods.midiOutGetVolume(handle, ref volume);
                    volumeRead = true;
                }
                return volume;
            }

            set
            {
                if (NativeMethods.midiOutSetVolume(handle, value) == EMMError.NOERROR)
                {
                    volume = value;
                }
            }
        }
        #endregion

        #region Public methods

        // Open the input port.
        public EMMError Open()
        {
            return Open(null);
        }

        // Open the input port. callback is for non-midi events such as done playing etc
        public EMMError Open(MidiEventHandler callback)
        {
            // Memorize the user callback
            this.callback = callback;

            NativeMethods.MidiOutProc nativeCallback = (callback == null) ? null : midiOutProc;
            int flags = (callback == null) ? 0 : NativeMethods.CALLBACK_FUNCTION;

            // Open the port
            var st = NativeMethods.midiOutOpen(
                out handle,
                id,
                nativeCallback,
                IntPtr.Zero,
                flags);

            return st;
        }

        // Reset the port
        public EMMError Reset()
        {
            return NativeMethods.midiOutReset(handle);
        }

        // Forward callback to user-provided function
        private void MidiProc(IntPtr hMidiIn,
            EMMMidiMessages wMsg,
            IntPtr dwInstance,
            uint dwParam1,
            uint dwParam2)
        {
            if ((callback != null) && (wMsg == EMMMidiMessages.MIM_DATA))
            {
                // Make a midi event out of the incoming params
                MidiEvent e = MidiInParser.ParseMimDataMessage(dwParam1, dwParam2);

                // Give it to the user
                callback(this, new MidiEventArgs(e, dwParam1, dwParam2));
            }
        }

        // Send a message
        public EMMError Send(MidiMessage m)
        {
            var st = EMMError.NOERROR;

            if (m is MidiSysexMessage)
            {
                // Sysex messages are sent using a buffer.
                var sysex = m as MidiSysexMessage;
                var buffer = new MidiBuffer(sysex.Body);
                using (buffer)
                {
                    // Prepare the buffer
                    st = buffer.PrepareForMidiOut(handle);
                    if (st == EMMError.NOERROR)
                    {
                        // Send the sysex
                        st = NativeMethods.midiOutLongMsg(handle, buffer.Header, buffer.HeaderSize);
                    }
                    if (st == EMMError.NOERROR)
                    {
                        // Wait until it is sent
                        // Note: this is simple but weak... If performance becomes necessary
                        // we can declare a callback, not wait here (just queue the buffer) and
                        // unprepare it when we receive the appropriate message in the callback 
                        while (!buffer.IsFree())
                        {
                            Thread.Sleep(1);
                        }

                        // unprepare the buffer
                        st = buffer.UnprepareForMidiOut(handle);
                    }
                }
            }
            else
            {
                uint sm = MidiOutBuilder.BuildShortMessage(m);
                st = NativeMethods.midiOutShortMsg(handle, sm);
            }

            return st;
        }

        // Close the port
        public EMMError Close()
        {
            EMMError result = NativeMethods.midiOutClose(handle);
            handle = IntPtr.Zero;
            return result;
        }

        #endregion

        #region Receiving events from the sequencer

        public void SequencerEventHandler(SequencerEventArg arg)
        {
            switch (arg.Type)
            {
                case SequencerEventArg.EType.Play:
                    // Play an event.
                    var midiEvent = arg.Event;
                    if (midiEvent != null)
                    {
                        // I only know how to play midi events. Override this method to play other events
                        var midiMessage = arg.Event.SequencerMessage as MidiMessage;
                        if (midiMessage != null)
                        {
                            Send(midiMessage);
                        }
                    }
                    break;
                case SequencerEventArg.EType.Reset:
                    // All notes off
                    Reset();
                    break;
                case SequencerEventArg.EType.End:
                    // End of sequence: nothing to do
                    break;
            }
        }

        public void AllNotesOff()
        {
            Reset();
        }

        public void PlaySequencerEvent(ISequencerMessage sequencerMessage)
        {
            // I only know how to play midi events
            var midiMessage = sequencerMessage as MidiMessage;
            if (midiMessage != null)
            {
                Send(midiMessage);
            }
        }

        #endregion

        #region Native method translation layer

        private void GetAndParseMidiOutCaps()
        {
            // Prepare parameters
            IntPtr pId = (IntPtr)id;
            NativeMethods.MIDIOUTCAPS caps = new NativeMethods.MIDIOUTCAPS();
            uint sz = (uint)Marshal.SizeOf(typeof(NativeMethods.MIDIOUTCAPS));

            // Call native function
            NativeMethods.midiOutGetDevCaps(pId, caps, sz);

            // Parse results
            manufacturerId = caps.wMid;
            productId = caps.wPid;
            name = caps.szPname;
            technology = (ETechnology)caps.wTechnology;
            maxVoices = caps.wVoices;
            maxNotes = caps.wNotes;
            channelMask = caps.wChannelMask;
            driverVersion = String.Format("{0}.{1}", caps.vDriverVersion >> 8, caps.vDriverVersion & 0xff);
        }

        #endregion

        #region Access to Windows Multi-media "C" API

        private static class NativeMethods
        {
            public enum ETechnology : ushort { MOD_MIDIPORT = 1,	MOD_SYNTH, MOD_SQSYNTH, MOD_FMSYNTH, MOD_MAPPER, MOD_WAVETABLE, MOD_SWSYNTH };

            /// <summary>
            /// C# implementation of MIDIOUTCAPS, returned by midiOutGetDevCaps()
            /// typedef struct {
            ///     WORD        wMid;
            ///     WORD        wpid;
            ///     MMVERSION   vDriverVersion;
            ///     TCHAR       szPname[MAXPNAMELEN];
            ///     WORD        wTechnology;
            ///     WORD        wVoices;
            ///     WORD        wNotes;
            ///     WORD        wChannelMask;
            ///     DWORD       dwSupport;
            ///  }
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            internal class MIDIOUTCAPS
            {
                const int MAXPNAMELEN = 32;

                internal ushort wMid;

                internal ushort wPid;

                internal uint vDriverVersion;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
                internal string szPname;

                internal ETechnology wTechnology;

                internal ushort wVoices;
                
                internal ushort wNotes;
                
                internal ushort wChannelMask;
                
                internal uint dwSupport;
            }

            internal const int CALLBACK_FUNCTION = 0x00030000;

            // Callback type expected by midiOutOpen()
            internal delegate void MidiOutProc(
                IntPtr hMidiOut,
                EMMMidiMessages wMsg,
                IntPtr dwInstance,
                uint dwParam1,
                uint dwParam2);

            [DllImport("winmm.dll")]
            internal static extern uint midiOutGetNumDevs();

            [DllImport("winmm.dll")]
            internal static extern EMMError midiOutGetDevCaps(IntPtr uDeviceId, [In, Out] MIDIOUTCAPS lpMidiInCaps, uint cbMidiInCaps);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiOutClose(IntPtr hMidiOut);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiOutOpen(
                out IntPtr lphMidiOut,
                uint uDeviceID,
                MidiOutProc dwCallback,
                IntPtr dwCallbackInstance,
                int dwFlags);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiOutGetVolume(IntPtr hMidiOut, ref uint lpdwVolume);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiOutSetVolume(IntPtr hMidiOut, uint lpdwVolume);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiOutReset(IntPtr hMidiOut);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiOutLongMsg(IntPtr hMidiOut, IntPtr lpMidiOutHdr, uint cbMidiOutHdr);
        }
        #endregion
    }
}
