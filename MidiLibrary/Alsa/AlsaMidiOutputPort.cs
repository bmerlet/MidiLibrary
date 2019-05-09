using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MidiLibrary.PortIO;

namespace MidiLibrary.Alsa
{
    public class AlsaMidiOutputPort : IMidiOutputPort, IDisposable
    {
        #region Static utilities

        /// <summary>
        /// Returns all midi output ports
        /// </summary>
        /// <returns>Array containing every midi input port defined in the system</returns>
        static public AlsaMidiOutputPort[] GetAllPorts()
        {
            var devices = AlsaUtils.EnumerateMidiPorts(AlsaNativeMethods.ESnd_rawmidi_stream.SND_RAWMIDI_STREAM_OUTPUT);
            var ports = new List<AlsaMidiOutputPort>();
            foreach (var dev in devices)
            {
                ports.Add(new AlsaMidiOutputPort(dev.Device, dev.Name));
            }
            return ports.ToArray();
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
        /// <param name="device">Device name of the port, e.g. hw:0,1,1</param>
        /// <param name="name">Human-readable name of the port, e.g. AKM320</param>
        private AlsaMidiOutputPort(string device, string name)
        {
            // Memorize input
            Device = device;
            Name = name;

            // Init
            handle = IntPtr.Zero;
            volumeRead = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (handle != IntPtr.Zero)
            {
                //WindowsNativeMethods.midiOutClose(handle);
                handle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AlsaMidiOutputPort()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        // Device name, e.g. hw:1,0,0
        public string Device { get; }

        // Port name, e.g. AKM320 MIDI 1
        public string Name { get; }

        // Volume
        public uint Volume
        {
            get
            {
                if (!volumeRead)
                {
                    //NativeMethods.midiOutGetVolume(handle, ref volume);
                    volumeRead = true;
                }
                return volume;
            }

            set
            {
                //if (NativeMethods.midiOutSetVolume(handle, value) == EMMError.NOERROR)
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
            var st = AlsaNativeMethods.Snd_rawmidi_open_output(
                    IntPtr.Zero, ref handle, Device, AlsaNativeMethods.EMode.SND_RAWMIDI_NONBLOCK);

            return AlsaUtils.StrError(st);
        }

        // Reset the port
        public string Reset()
        {
            int st = AlsaNativeMethods.Snd_rawmidi_drain(handle);
            return AlsaUtils.StrError(st);
        }

        // Send a message
        public string Send(MidiMessage m)
        {
            byte[] data = m.GetAsByteArray();

            // Meta messages are not sent over the wire
            if (data == null)
            {
                return null; 
            }

            ulong size = (ulong)data.Length;

            long st = AlsaNativeMethods.Snd_rawmidi_write(handle, data, size);

            return AlsaUtils.StrError((int)st);
        }

        // Close the port
        public string Close()
        {
            int st = 0;

            if (handle != IntPtr.Zero)
            {
                Reset();
                st = AlsaNativeMethods.Snd_rawmidi_close(handle);
            }
            handle = IntPtr.Zero;

            return AlsaUtils.StrError(st);
        }

        #endregion
    }
}
