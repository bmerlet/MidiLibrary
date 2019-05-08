using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MidiLibrary.Alsa
{
    internal static class AlsaUtils
    {
        static public string StrError(int err)
        {
            string result = null;

            if (err < 0)
            {
                var ptr = AlsaNativeMethods.Snd_strerror(err);
                result = Marshal.PtrToStringAuto(ptr);
            }

            return result;
        }

        static public DeviceDescription[] EnumerateMidiPorts(AlsaNativeMethods.ESnd_rawmidi_stream type)
        {
            var ports = new List<DeviceDescription>();

            // Enumerate all the sound cards
            int soundCardIx = -1;
            while (true)
            {
                // Get next sound card
                int err = AlsaNativeMethods.Snd_card_next(ref soundCardIx);
                if (err < 0)
                {
                    throw new InvalidOperationException("Cannot get next sound card err = " + StrError(err));
                }

                // No more sound card (normal end)
                if (soundCardIx < 0)
                {
                    break;
                }

                // Build sound card name
                string soundCardName = "hw:" + soundCardIx.ToString();

                // Open the sound card
                IntPtr handlePtr = IntPtr.Zero;
                err = AlsaNativeMethods.Snd_ctl_open(ref handlePtr, soundCardName, 0);
                if (err < 0)
                {
                    throw new InvalidOperationException(
                        "Cannot open sound card " + soundCardName + ", " + StrError(err));
                }

                // Get all midi out devices on this sound card
                int deviceIx = -1;
                while (true)
                {
                    err = AlsaNativeMethods.Snd_ctl_rawmidi_next_device(handlePtr, ref deviceIx);
                    if (err < 0)
                    {
                        throw new InvalidOperationException(
                            "Cannot get next midi device on sound card " + soundCardName + ", " + StrError(err));
                    }

                    // End of devices
                    if (deviceIx < 0)
                    {
                        break;
                    }

                    // We have a device!
                    //Console.WriteLine("Found device " + deviceIx + " on soundcard " + soundCardName);

                    // Get info block
                    IntPtr infoPtr = IntPtr.Zero;
                    AlsaNativeMethods.Snd_rawmidi_info_malloc(ref infoPtr);

                    // Set device id in info block
                    AlsaNativeMethods.Snd_rawmidi_info_set_device(infoPtr, (uint)deviceIx);

                    // Set stream type (i.e. input/output)
                    AlsaNativeMethods.Snd_rawmidi_info_set_stream(infoPtr, type);

                    // Get info
                    err = AlsaNativeMethods.Snd_ctl_rawmidi_info(handlePtr, infoPtr);
                    if (err >= 0)
                    {
                        // We have a midi device of the type we are looking for!

                        // Get number of sub-devices
                        int subDeviceCount = AlsaNativeMethods.Snd_rawmidi_info_get_subdevices_count(infoPtr);

                        // Iterate on sub-devices
                        for (uint subDevice = 0; subDevice < subDeviceCount; subDevice++)
                        {
                            AlsaNativeMethods.Snd_rawmidi_info_set_subdevice(infoPtr, subDevice);
                            err = AlsaNativeMethods.Snd_ctl_rawmidi_info(handlePtr, infoPtr);
                            if (err < 0)
                            {
                                throw new InvalidOperationException(
                                    "Cannot get raw midi info err = " + StrError(err));
                            }

                            var namePtr = AlsaNativeMethods.Snd_rawmidi_info_get_name(infoPtr);
                            var name = Marshal.PtrToStringAuto(namePtr);
                            var subNamePtr = AlsaNativeMethods.Snd_rawmidi_info_get_subdevice_name(infoPtr);
                            var subName = Marshal.PtrToStringAuto(subNamePtr);

                            string portName =
                             (subDevice == 0 && subName == "") ?
                             soundCardName + "," + deviceIx :
                             soundCardName + "," + deviceIx + "," + subDevice;
                            name = (subDevice == 0 && subName == "") ? name : subName;

                            ports.Add(new DeviceDescription(portName, name));
                        }
                    }

                    // Free info block
                    AlsaNativeMethods.Snd_rawmidi_info_free(infoPtr);
                }

                // Close sound card
                AlsaNativeMethods.Snd_ctl_close(handlePtr);

            }
            return ports.ToArray();
        }

        internal class DeviceDescription
        {
            public string Device { get; }
            public string Name { get; }

            public DeviceDescription(string device, string name)
            {
                Device = device;
                Name = name;
            }
        }
    }
}
