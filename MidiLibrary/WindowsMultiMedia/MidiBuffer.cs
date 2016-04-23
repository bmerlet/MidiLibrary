//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MidiLibrary.WindowsMultiMedia
{
    /// <summary>
    /// Class representing a midi buffer, used to transport sysex messages
    /// </summary>
    class MidiBuffer : IDisposable
    {
        #region Private members

        // Flags for dwFlags field of MIDIHDR structure
        [Flags]
        private enum MHDR : uint {
            MHDR_DONE = 1, // Done bit
            MHDR_PREPARED = 2, // Set if header is prepared
            MHDR_INQUEUE = 4, // reserved for driver
            MHDR_ISSTRM = 8 // Buffer is stream buffer
        };

        // Pointer to the midi header in umnmanaged space
        private IntPtr header;

        // Header size, computed once and for all
        private uint headerSize;

        // Pointer to the buffer itself, in unmanaged space
        private IntPtr buffer;

        // If the buffer was prepared for in/out
        private bool preparedForMidiIn;
        private bool preparedForMidiOut;

        #endregion

        #region Constructor/Destructor

        public MidiBuffer(byte[] data)
            : this(data.Length)
        {
            Marshal.Copy(data, 0, buffer, data.Length); 
        }

        public MidiBuffer(int bufferSize)
            : this(bufferSize, IntPtr.Zero)
        {
        }

        public MidiBuffer(int bufferSize, IntPtr userData)
        {
            // Allocate unmanaged buffer
            this.buffer = Marshal.AllocHGlobal(bufferSize);

            // Allocate unmanaged buffer descriptor
            this.header = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.MIDIHDR)));
            this.headerSize = (uint)Marshal.SizeOf(typeof(NativeMethods.MIDIHDR));
            this.preparedForMidiIn = false;
            this.preparedForMidiOut = false;

            // Build a buffer descriptor in the managed space
            var managedHeader = new NativeMethods.MIDIHDR();
            managedHeader.lpData = buffer;
            managedHeader.dwBufferLength = (uint)bufferSize;
            managedHeader.dwBytesRecorded = 0;
            managedHeader.dwUser = userData;
            managedHeader.dwFlags = 0; // Required to be zero before calling the prepare function
            managedHeader.lpNext = IntPtr.Zero;
            managedHeader.reserved = IntPtr.Zero;
            managedHeader.dwOffset = 0;

            // Copy the managed object into the newly created unmanaged buffer
            Marshal.StructureToPtr(managedHeader, header, false);
        }

        // Destroy a midi buffer
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool managedAlso)
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;
            }

            if (header != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(header);
                header = IntPtr.Zero;
            }

            // We only have native resources to clean up
            // if (managedAlso)
            // {
            //     ... Dispose managed resources
            // }
        }

        #endregion

        #region Properties

        public IntPtr Header
        {
            get { return header; }
        }

        public IntPtr Buffer
        {
            get { return buffer; }
        }

        public uint HeaderSize
        {
            get { return headerSize; }
        }

        public bool PreparedForMidiOut
        {
            get { return preparedForMidiOut; }
        }

        public bool PreparedForMidiIn
        {
            get { return preparedForMidiIn; }
        }

        #endregion

        #region MidiOut-oriented public methods

        // Prepare a buffer for an output port
        public EMMError PrepareForMidiOut(IntPtr handle)
        {
            // Prepare the buffer
            var st = NativeMethods.midiOutPrepareHeader(handle, header, (uint)Marshal.SizeOf(typeof(NativeMethods.MIDIHDR)));
            if (st == EMMError.NOERROR)
            {
                preparedForMidiOut = true;
            }

            return st;
        }

        // Test if a buffer is free (i.e. it belongs to us again)
        public bool IsFree()
        {
            // Copy the header to managed code
            var managedHeader = (NativeMethods.MIDIHDR)Marshal.PtrToStructure(header, typeof(NativeMethods.MIDIHDR));

            // test the flags
            return (managedHeader.dwFlags & (uint)MHDR.MHDR_DONE) != 0;
        }

        public EMMError UnprepareForMidiOut(IntPtr handle)
        {
            EMMError st = EMMError.DELETEERROR;

            // Un-prepare the buffer
            if (preparedForMidiOut)
            {
                st = NativeMethods.midiOutUnprepareHeader(handle, header, (uint)Marshal.SizeOf(typeof(NativeMethods.MIDIHDR)));
                if (st == EMMError.NOERROR)
                {
                    preparedForMidiOut = false;
                }
            }

            return st;
        }


        #endregion

        #region MidiIn-oriented public methods

        // Add this buffer to a midi in port
        public EMMError AddMidiInBuffer(IntPtr handle)
        {
            // Prepare the buffer
            EMMError st = NativeMethods.midiInPrepareHeader(handle, header, headerSize);
            if (st != EMMError.NOERROR)
            {
                return st;
            }

            preparedForMidiIn = true;

            // Give it to windows
            st = NativeMethods.midiInAddBuffer(handle, header, headerSize);
            if (st != EMMError.NOERROR)
            {
                return st;
            }

            return EMMError.NOERROR;
        }

        // Remove this buffer from a midi in port
        public EMMError RemoveMidiInBuffer(IntPtr handle)
        {
            EMMError st = EMMError.DELETEERROR;

            if (preparedForMidiIn)
            {
                st = NativeMethods.midiInUnprepareHeader(handle, header, (uint)Marshal.SizeOf(typeof(NativeMethods.MIDIHDR)));
                if (st == EMMError.NOERROR)
                {
                    preparedForMidiIn = false;
                }
            }

            return st;
        }

        // Get the user data from a header pointer
        static public IntPtr GetUserData(IntPtr header)
        {
            // Copy the header to managed code
            var managedHeader = (NativeMethods.MIDIHDR)Marshal.PtrToStructure(header, typeof(NativeMethods.MIDIHDR));

            // Get the requested value
            return managedHeader.dwUser;
        }

        // Get the recorded data
        public byte[] GetRecordedData()
        {
            // Copy the header to managed code
            var managedHeader = (NativeMethods.MIDIHDR)Marshal.PtrToStructure(header, typeof(NativeMethods.MIDIHDR));

            // Allocate array to receive the data
            var data = new byte[managedHeader.dwBytesRecorded];

            // Copy it from the unmanaged buffer
            Marshal.Copy(managedHeader.lpData, data, 0, data.Length);

            return data;
        }

        #endregion

        #region Access to Windows Multi-media "C" API

        private static class NativeMethods
        {
            /// <summary>
            /// C# implementation of MIDIHDR, used to describe buffers for sysex
            /// typedef struct midihdr_tag {
            ///   LPSTR              lpData;
            ///   DWORD              dwBufferLength;
            ///   DWORD              dwBytesRecorded;
            ///   DWORD_PTR          dwUser;
            ///   DWORD              dwFlags;
            ///   struct midihdr_tag  *lpNext;
            ///   DWORD_PTR          reserved;
            ///   DWORD              dwOffset;
            ///   DWORD_PTR          dwReserved[4];
            /// } MIDIHDR, *LPMIDIHDR;
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MIDIHDR
            {
                internal IntPtr lpData;
                internal uint dwBufferLength;
                internal uint dwBytesRecorded;
                internal IntPtr dwUser;
                internal uint dwFlags;
                internal IntPtr lpNext;
                internal IntPtr reserved;
                internal uint dwOffset;
                internal IntPtr dwReserved1;
                internal IntPtr dwReserved2;
                internal IntPtr dwReserved3;
                internal IntPtr dwReserved4;
            }

            [DllImport("winmm.dll")]
            internal static extern EMMError midiOutPrepareHeader(IntPtr hMidiIn, IntPtr lpMidInHdr, uint cbMidiInHdr);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiOutUnprepareHeader(IntPtr hMidiIn, IntPtr lpMidInHdr, uint cbMidiInHdr);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiInPrepareHeader(IntPtr hMidiIn, IntPtr lpMidInHdr, uint cbMidiInHdr);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiInUnprepareHeader(IntPtr hMidiIn, IntPtr lpMidInHdr, uint cbMidiInHdr);

            [DllImport("winmm.dll")]
            internal static extern EMMError midiInAddBuffer(IntPtr hMidiIn, IntPtr lpMidInHdr, uint cbMidiInHdr);

        }
        #endregion
    }
}
