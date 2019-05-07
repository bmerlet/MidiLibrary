//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MidiLibrary.PortIO;

namespace MidiLibrary.WindowsMultiMedia
{
    public class WindowsMidiEventArgs : EventArgs, IMidiEventArgs 
    {
        public WindowsMidiEventArgs(MidiEvent midiEvent, IntPtr p1, IntPtr p2)
        {
            this.MidiEvent = midiEvent;
            this.P1 = p1;
            this.P2 = p2;
        }

        public MidiEvent MidiEvent { get; }

        // When originating from windows, the message contains dwParam1 and dwParam2
        public IntPtr P1 { get; }
        public IntPtr P2 { get; }
    }
}
