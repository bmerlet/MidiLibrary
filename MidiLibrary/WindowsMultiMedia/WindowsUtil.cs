using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.WindowsMultiMedia
{
    /// <summary>
    /// Windows multi-media Midi messages
    /// </summary>
    internal enum EMMMidiMessages : uint
    {
        MIM_OPEN = 0x3C1,
        MIM_CLOSE = 0x3C2,
        MIM_DATA = 0x3C3,
        MIMLONG_DATA = 0x3C4,
        MIM_ERROR = 0x3C5,
        MIMLONG_ERROR = 0x3C6,
        MOM_OPEN = 0x3C7,
        MOM_CLOSE = 0x3C8,
        MOM_DONE = 0x3C9
    }

    // Enum equivalent to MMSYSERR_*
    internal enum EMMError : int
    {
        NOERROR = 0,
        ERROR = 1,
        BADDEVICEID = 2,
        NOTENABLED = 3,
        ALLOCATED = 4,
        INVALHANDLE = 5,
        NODRIVER = 6,
        NOMEM = 7,
        NOTSUPPORTED = 8,
        BADERRNUM = 9,
        INVALFLAG = 10,
        INVALPARAM = 11,
        HANDLEBUSY = 12,
        INVALIDALIAS = 13,
        BADDB = 14,
        KEYNOTFOUND = 15,
        READERROR = 16,
        WRITEERROR = 17,
        DELETEERROR = 18,
        VALNOTFOUND = 19,
        NODRIVERCB = 20,
        WAVEERR_BADFORMAT = 32,
        WAVEERR_STILLPLAYING = 33,
        WAVEERR_UNPREPARED = 34,
        WAVEERR_SYNC = 35
    }

    internal class WindowsUtil
    {
        public static string StrError(EMMError error)
        {
            if (error == EMMError.NOERROR)
            {
                return null;
            }
            else
            {
                return error.ToString();
            }
        }
    }
}
