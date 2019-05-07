using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.PortIO
{
    public interface IMidiEventArgs
    {
        MidiEvent MidiEvent { get; }
    }
}
