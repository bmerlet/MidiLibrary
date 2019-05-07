using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MidiLibrary.WindowsMultiMedia;

namespace MidiLibrary.PortIO
{

    public class PortEnumerator
    {
        static public IMidiOutputPort[] OutputPorts
        {
            get
            {
                return WindowsMidiOutputPort.GetAllPorts();
            }
        }
    }
}
