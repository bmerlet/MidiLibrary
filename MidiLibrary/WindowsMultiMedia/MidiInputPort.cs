//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Linq;
using System.Runtime.InteropServices;
using MidiLibrary;
using MidiLibrary.SysexMessages;

namespace MidiLibrary.WindowsMultiMedia
{
    public class MidiInputPort : IDisposable
    {
        #region Static utilities

        /// <summary>
        /// Get the number of midi input port
        /// </summary>
        public static uint Count
        {
            get { return NativeMethods.midiInGetNumDevs(); }
        }

        /// <summary>
        /// Returns all midi input ports
        /// </summary>
        /// <returns>Array containing every midi input port defined in the system</returns>
        static public MidiInputPort[] GetAllPorts()
        {
            uint numDevices = Count;
            MidiInputPort[] devs = new MidiInputPort[numDevices];

            for (uint d = 0; d < numDevices; d++)
            {
                devs[d] = new MidiInputPort(d);
            }

            return devs;
        }

        #endregion

        #region Events

        public event MidiEventHandler MidiInputReceived;

        #endregion

        #region Private members

        // Device id
        private uint id;

        // Properties of the device
        private ushort manufacturerId;
        private ushort productId;
        private string driverVersion;
        private string name;

        // Handle when this device is opened
        private IntPtr handle;

        // Callback function called by the native code, which in turn raises the MidiInputReceived event
        private NativeMethods.MidiInProc midiInProc;

        // Buffers for sysex
        private MidiBuffer[] buffers;

        // Partial sysex data being received
        private byte[] partialSysex;

        #endregion

        #region Constructor & Destructor
        
        /// <summary>
        /// Construct a new midi input port.
        /// </summary>
        /// <param name="id">Id of the port, between 0 and the number returned by InputCount</param>
        public MidiInputPort(uint id)
        {
            // Id of this port
            this.id = id;

            // Native callback calling MidiProc() 
            midiInProc = new NativeMethods.MidiInProc(MidiProc);

            // Init
            this.handle = IntPtr.Zero;
            this.buffers = null;
            this.partialSysex = null;

            // Get the capabilities of this device
            GetAndParseMidiInCaps();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool managedAlso)
        {
            // Dispose native resources
            if (handle != IntPtr.Zero)
            {
                Close();
                handle = IntPtr.Zero;
            }

            // We only have native resources to clean up
            // if (managedAlso)
            // {
            //     ... Dispose managed resources
            // }
        }

        ~MidiInputPort()
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

        public String DriverVersion
        {
            get { return driverVersion; }
        }

        public String Name
        {
            get { return name; }
        }

        #endregion

        #region Public methods

        // Open the input port. Midi events cause the MidiInputReceived event to be raised
        public EMMError Open()
        {
            // Open the port
            return NativeMethods.midiInOpen(
                out handle,
                id,
                midiInProc,
                IntPtr.Zero,
                NativeMethods.CALLBACK_FUNCTION);
        }

        // Start the port and setup buffers to receives Sysex
        public EMMError Start()
        {
            // Allocate buffer array
            if (buffers == null)
            {
                const int numBuffers = 4;
                const int bufferSize = 4096; // ZZZ Param?

                buffers = new MidiBuffer[numBuffers];

                // Allocate and prepare the buffers
                for (int b = 0; b < buffers.Length; b++)
                {
                    // Create buffer
                    buffers[b] = new MidiBuffer(bufferSize, (IntPtr)b);

                    // Prepare the buffer and give it to windows
                    EMMError st = buffers[b].AddMidiInBuffer(handle);
                    if (st != EMMError.NOERROR)
                    {
                        return st;
                    }
                }
            }

            // Finally, start the port
            return NativeMethods.midiInStart(handle);
        }

        // Forward callback to user-provided function
        private void MidiProc(IntPtr hMidiIn,
            EMMMidiMessages wMsg,
            IntPtr dwInstance,
            IntPtr dwParam1,
            IntPtr dwParam2)
        {
            if (wMsg == EMMMidiMessages.MIM_DATA)
            {
                if (MidiInputReceived != null)
                {
                    // Make a midi event out of the incoming params
                    MidiEvent e = MidiInParser.ParseMimDataMessage((uint)dwParam1, (uint)dwParam2);

                    // Give it to the user
                    MidiInputReceived.Invoke(this, new MidiEventArgs(e, dwParam1, dwParam2));
                }
            }
            else if (wMsg == EMMMidiMessages.MIMLONG_DATA)
            {
                // The first parameter is a pointer to a buffer descriptor
                var header = dwParam1;

                // Find this buffer in our buffer set
                int index = (int)MidiBuffer.GetUserData(header);
                if (index < 0 || index >= buffers.Length)
                {
                    throw new InvalidOperationException("midiInputPort: MIMLONG_DATA buffer not found - index " + index);
                }
                var buffer = buffers[index];

                // Get data from buffer
                var data = buffer.GetRecordedData();
                if (data.Length == 0)
                {
                    // This happens on Reset(), all the buffers are freed by windows and we get a MIMLONG_DATA for each of them.
                    // Free the buffer.
                    buffer.RemoveMidiInBuffer(handle);
                    buffer.Dispose();
                    buffers[index] = null;
                }
                else
                {
                    // Give the buffer back to windows, we are done with it
                    buffer.AddMidiInBuffer(handle);

                    // If we have a callback, either generate a sysex or
                    // memorize the data if we don't have a full sysex
                    if (MidiInputReceived != null)
                    {
                        MidiSysexMessage sysex = null;

                        if (data[0] == (byte)EMidiCommand.SystemExclusive)
                        {
                            // This buffer contains the beginning of the sysex - check we don't have an ongoing sysex
                            if (this.partialSysex != null)
                            {
                                throw new InvalidOperationException("midiInputPort: Beginning of new sysex received while previous one still on-going");
                            }

                            if (data[data.Length - 1] == (byte)EMidiCommand.EndOfSystemExclusive)
                            {
                                // This buffer contains a full sysex
                                sysex = new MidiSysexMessage(data);
                            }
                            else
                            {
                                // This buffer contains the beginning of a sysex
                                this.partialSysex = data;
                            }
                        }
                        else
                        {
                            // This buffer is a continuation of a sysex. Verify we have an ongoing sysex
                            if (this.partialSysex == null)
                            {
                                throw new InvalidOperationException("midiInputPort: Continuation sysex without beginning");
                            }

                            // Concatenate the partial data and the new data
                            var dataSoFar = new byte[partialSysex.Length + data.Length];
                            partialSysex.CopyTo(dataSoFar, 0);
                            data.CopyTo(dataSoFar, dataSoFar.Length);

                            if (data[data.Length - 1] == (byte)EMidiCommand.EndOfSystemExclusive)
                            {
                                // This buffer concludes the sysex
                                sysex = new MidiSysexMessage(dataSoFar);
                                this.partialSysex = null;
                            }
                            else
                            {
                                // This buffer contains a partial sysex, add it to the partial sysex
                                this.partialSysex = dataSoFar;
                            }
                        }
 
                        // Create sysex event if we have a sysex
                        if (sysex != null)
                        {
                            // Make a midi event out of the sysex
                            var e = new MidiEvent(sysex, (uint)dwParam2);

                            // Give it to the user
                            MidiInputReceived.Invoke(this, new MidiEventArgs(e, dwParam1, dwParam2));
                        }
                    }
                }
            }
        }

        // Stop the port
        public EMMError Stop()
        {
            return NativeMethods.midiInStop(handle);
        }

        // Reset the port and free the sysex buffers
        public EMMError Reset()
        {
            EMMError st = NativeMethods.midiInReset(handle);

            if (st == EMMError.NOERROR && buffers != null)
            {
                // The buffers have now been released by the callback. Just free the array.
                buffers = null;
            }

            return st;
        }

        // Close the port
        public EMMError Close()
        {
            Stop();
            Reset();
            EMMError result = NativeMethods.midiInClose(handle);
            handle = IntPtr.Zero;
            MidiInputReceived = null;
            return result;
        }

        #endregion

        #region Native method translation layer

        private void GetAndParseMidiInCaps()
        {
            // Prepare parameters
            IntPtr pId = (IntPtr)id;
            NativeMethods.MIDIINCAPS caps = new NativeMethods.MIDIINCAPS();
            uint sz = (uint)Marshal.SizeOf(typeof(NativeMethods.MIDIINCAPS));

            // Call native function
            NativeMethods.midiInGetDevCaps(pId, caps, sz);

            // Parse results
            manufacturerId = caps.wMid;
            productId = caps.wPid;
            driverVersion = String.Format("{0}.{1}", caps.vDriverVersion >> 8, caps.vDriverVersion & 0xff);
            name = caps.szPname;
        }

        #endregion

        #region Access to Windows Multi-media "C" API

        private static class NativeMethods
        {
            /// <summary>
            /// C# implementation of MIDIINCAPS, returned by midiInGetDevCaps()
            /// typedef struct {
            ///     WORD        wMid;
            ///     WORD        wpid;
            ///     MMVERSION   vDriverVersion;
            ///     TCHAR       szPname[MAXPNAMELEN];
            ///     DWORD       dwSupport;
            ///  }
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            internal class MIDIINCAPS
            {
                const int MAXPNAMELEN = 32;

                internal ushort wMid;

                internal ushort wPid;

                internal uint vDriverVersion;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
                internal string szPname;

                internal uint dwSupport;
            }

            internal const int CALLBACK_FUNCTION = 0x00030000;

            // Callback type expected by midiInOpen()
            internal delegate void MidiInProc(
                IntPtr hMidiIn,
                EMMMidiMessages wMsg,
                IntPtr dwInstance,
                IntPtr dwParam1,
                IntPtr dwParam2);

            [DllImport("winmm.dll")]
            internal static extern uint midiInGetNumDevs();

            [DllImport("winmm.dll")]
            internal static extern EMMError midiInGetDevCaps(IntPtr uDeviceId, [In, Out] MIDIINCAPS lpMidiInCaps, uint cbMidiInCaps);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiInClose(IntPtr hMidiIn);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiInOpen(
                out IntPtr lphMidiIn,
                uint uDeviceID,
                MidiInProc dwCallback,
                IntPtr dwCallbackInstance,
                int dwFlags);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiInStart(IntPtr hMidiIn);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiInStop(IntPtr hMidiIn);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiInReset(IntPtr hMidiIn);
        }
        #endregion
    }
}
