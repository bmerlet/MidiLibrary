﻿//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MidiLibrary.PortIO;
using MidiLibrary.SysexMessages;
using MidiLibrary.Sequencer;

namespace MidiLibrary.WindowsMultiMedia
{
    public class WindowsMidiOutputPort : IMidiOutputPort, IDisposable
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
        static internal WindowsMidiOutputPort[] GetAllPorts()
        {
            uint numDevices = Count;
            WindowsMidiOutputPort[] devs = new WindowsMidiOutputPort[numDevices];

            for (uint d = 0; d < numDevices; d++)
            {
                devs[d] = new WindowsMidiOutputPort(d);
            }

            return devs;
        }

        #endregion

        #region Private members

        // Handle when this device is opened
        private IntPtr handle;

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
        private WindowsMidiOutputPort(uint id)
        {
            // Id of this port
            this.Id = id;

            // Init
            handle = IntPtr.Zero;
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

        ~WindowsMidiOutputPort()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public uint Id { get; }

        public ushort ManufacturerId { get; private set; }

        public ushort ProductId { get; private set; }

        public String Name { get; private set; }

        public ETechnology Technology { get; private set; }

        public ushort MaxVoices { get; private set; }

        public ushort MaxNotes { get; private set; }

        public ushort ChannelMask { get; private set; }

        public String DriverVersion { get; private set; }

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
        public string Open()
        {
            // Open the port
            var st = NativeMethods.midiOutOpen(
                out handle,
                Id,
                null,
                IntPtr.Zero,
                0);

            return WindowsUtil.StrError(st);
        }

        // Reset the port
        public string Reset()
        {
            return WindowsUtil.StrError(NativeMethods.midiOutReset(handle));
        }

        // Send a message
        public string Send(MidiMessage m)
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
                uint sm = m.GetAsShortMessage();
                if (sm != uint.MaxValue)
                {
                    st = NativeMethods.midiOutShortMsg(handle, sm);
                }
            }

            return WindowsUtil.StrError(st);
        }

        // Close the port
        public string Close()
        {
            EMMError result = NativeMethods.midiOutClose(handle);
            handle = IntPtr.Zero;
            return WindowsUtil.StrError(result);
        }

        #endregion

        #region Native method translation layer

        private void GetAndParseMidiOutCaps()
        {
            // Prepare parameters
            IntPtr pId = (IntPtr)Id;
            NativeMethods.MIDIOUTCAPS caps = new NativeMethods.MIDIOUTCAPS();
            uint sz = (uint)Marshal.SizeOf(typeof(NativeMethods.MIDIOUTCAPS));

            // Call native function
            NativeMethods.midiOutGetDevCaps(pId, caps, sz);

            // Parse results
            ManufacturerId = caps.wMid;
            ProductId = caps.wPid;
            Name = caps.szPname;
            Technology = (ETechnology)caps.wTechnology;
            MaxVoices = caps.wVoices;
            MaxNotes = caps.wNotes;
            ChannelMask = caps.wChannelMask;
            DriverVersion = String.Format("{0}.{1}", caps.vDriverVersion >> 8, caps.vDriverVersion & 0xff);
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
                IntPtr dwParam1,
                IntPtr dwParam2);

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
