using System;
using System.Runtime.InteropServices;

namespace MidiLibrary.Alsa
{
    internal static class AlsaNativeMethods
    {
        // Shared library
        private const string LibraryName = "libasound.so.2";

        // Size of a pollfd structure
        internal const int POLLFDSZ = 8;

        // Enums
        internal enum ESnd_rawmidi_stream : uint
        {
            SND_RAWMIDI_STREAM_OUTPUT = 0,
            SND_RAWMIDI_STREAM_INPUT = 1
        }

        [Flags]
        internal enum EMode : int
        {
            SND_RAWMIDI_APPEND = 0x0001,
            SND_RAWMIDI_NONBLOCK = 0x0002,
            SND_RAWMIDI_SYNC = 0x0004
        }

        // Error string
        // const char *snd_strerror (int errnum)
        [DllImport(LibraryName, EntryPoint = "snd_strerror")]
        internal extern static IntPtr Snd_strerror(int errnum);

        // Find the next sound card
        [DllImport(LibraryName, EntryPoint = "snd_card_next")]
        internal extern static int Snd_card_next(ref int card);

        // Open a sound card
        [DllImport(LibraryName, EntryPoint = "snd_ctl_open")]
        internal extern static int Snd_ctl_open(ref IntPtr handleRef, string soundCardName, int mode);

        // Open a midi device on a sound card
        [DllImport(LibraryName, EntryPoint = "snd_ctl_rawmidi_next_device")]
        internal extern static int Snd_ctl_rawmidi_next_device(IntPtr handleRef, ref int deviceIx);

        // Close a device on a sound card
        [DllImport(LibraryName, EntryPoint = "snd_ctl_rawmidi_next_device")]
        internal extern static int Snd_ctl_close(IntPtr handleRef);

        // Allocate info block for a raw midi device
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_info_malloc")]
        internal extern static int Snd_rawmidi_info_malloc(ref IntPtr infoPtrRef);

        // Set device in raw midi info block
        // void snd_rawmidi_info_set_device(snd_rawmidi_info_t* obj, unsigned int val);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_info_set_device")]
        internal extern static void Snd_rawmidi_info_set_device(IntPtr infoPtr, uint val);

        // Set stream type in raw midi info block
        // void snd_rawmidi_info_set_stream(snd_rawmidi_info_t *obj, snd_rawmidi_stream_t val);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_info_set_stream")]
        internal extern static void Snd_rawmidi_info_set_stream(IntPtr infoPtr, ESnd_rawmidi_stream val);

        // Get info on a raw midi device
        // int  snd_ctl_rawmidi_info (snd_ctl_t *ctl, snd_rawmidi_info_t *info);
        [DllImport(LibraryName, EntryPoint = "snd_ctl_rawmidi_info")]
        internal extern static int Snd_ctl_rawmidi_info(IntPtr ctlPtr, IntPtr infoPtr);

        // Get number of subdevices for a midi device
        // unsigned int snd_rawmidi_info_get_subdevices_count(const snd_rawmidi_info_t *obj);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_info_get_subdevices_count")]
        internal extern static int Snd_rawmidi_info_get_subdevices_count(IntPtr infoPtr);

        // Set sub device number in info block
        // void snd_rawmidi_info_set_subdevice(snd_rawmidi_info_t *obj, unsigned int val);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_info_set_subdevice")]
        internal extern static void Snd_rawmidi_info_set_subdevice(IntPtr infoPtr, uint val);

        // Get name of a raw midi device
        // const char *snd_rawmidi_info_get_name(const snd_rawmidi_info_t *obj);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_info_get_name")]
        internal extern static IntPtr Snd_rawmidi_info_get_name(IntPtr infoPtr);

        // Get name of a raw midi subdevice
        // const char *snd_rawmidi_info_get_subdevice_name(const snd_rawmidi_info_t *obj);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_info_get_subdevice_name")]
        internal extern static IntPtr Snd_rawmidi_info_get_subdevice_name(IntPtr infoPtr);

        // Free info block for a raw midi device
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_info_free")]
        internal extern static int Snd_rawmidi_info_free(IntPtr infoPtr);

        // Open a port
        // int snd_rawmidi_open(snd_rawmidi_t **in_rmidi, snd_rawmidi_t **out_rmidi, const char* name, int mode);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_open")]
        internal extern static int Snd_rawmidi_open_input_output(
            ref IntPtr rawMidiInPtrRef,
            ref IntPtr rawMidiOutPtrRef,
            string name,
            EMode mode);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_open")]
        internal extern static int Snd_rawmidi_open_input(
            ref IntPtr rawMidiInPtrRef,
            IntPtr zero,
            string name,
            EMode mode);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_open")]
        internal extern static int Snd_rawmidi_open_output(
            IntPtr zero,
            ref IntPtr rawMidiOutPtrRef,
            string name,
            EMode mode);

        // Close a port
        // int snd_rawmidi_close(snd_rawmidi_t *rmidi);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_close")]
        internal extern static int Snd_rawmidi_close(IntPtr rmidi);

        // Drain a port
        // int  snd_rawmidi_drain (snd_rawmidi_t *rmidi);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_drain")]
        internal extern static int Snd_rawmidi_drain(IntPtr rmidi);

        // Drop port events
        // int  snd_rawmidi_drop (snd_rawmidi_t *rmidi);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_drop")]
        internal extern static int Snd_rawmidi_drop(IntPtr rmidi);

        // Write to a port
        // ssize_t  snd_rawmidi_write (snd_rawmidi_t *rmidi, const void *buffer, size_t size);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_write")]
        internal extern static long Snd_rawmidi_write(IntPtr rmidi, byte[] buffer, ulong size);

        // Read from a port
        // ssize_t snd_rawmidi_read(snd_rawmidi_t *rmidi, void *buffer, size_t size);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_read")]
        internal extern static long Snd_rawmidi_read(IntPtr rmidi, byte[] buffer, ulong size);

        // Get count of poll structures
        // int  snd_rawmidi_poll_descriptors_count (snd_rawmidi_t *rmidi)
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_poll_descriptors_count")]
        internal extern static int Snd_rawmidi_poll_descriptors_count(IntPtr rmidi);

        // Fill out poll structures
        // int  snd_rawmidi_poll_descriptors (snd_rawmidi_t *rmidi, struct pollfd *pfds, unsigned int space)
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_poll_descriptors")]
        internal extern static int Snd_rawmidi_poll_descriptors(IntPtr rmidi, IntPtr poolfd, uint space);

        // Get events from poll structures
        // int  snd_rawmidi_poll_descriptors_revents (snd_rawmidi_t *rawmidi, struct pollfd *pfds, unsigned int nfds, unsigned short *revent);
        [DllImport(LibraryName, EntryPoint = "snd_rawmidi_poll_descriptors_revents")]
        internal static extern int Snd_rawmidi_poll_descriptors_revents(IntPtr rmidi, IntPtr poolfd, uint nfds, ref ushort revent);

        // poll()
        // int poll(struct pollfd *fds, nfds_t nfds, int timeout);
        [DllImport("libc.so.6", EntryPoint = "poll")]
        internal extern static int Poll(IntPtr fds, int nfds, int timeout);

        // Poll events
        [Flags]
        internal enum EPoll : ushort
        {
            POLLIN   =       0x0001,
            POLLPRI  =       0x0002,
            POLLOUT  =       0x0004,
            POLLERR  =       0x0008,
            POLLHUP  =       0x0010,
            POLLNVAL =       0x0020
        }
    }
}
