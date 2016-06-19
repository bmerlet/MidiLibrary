//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.WindowsMultiMedia
{
    // Generic delegate to handle a midi event
    public delegate void MidiEventHandler(object sender, MidiEventArgs arg);

    public class MidiEventArgs : EventArgs
    {
        // The midi event
        private MidiEvent midiEvent;

        // When originating from windows, the raw message content dwParam1/dwParam2
        private IntPtr p1;
        private IntPtr p2;

        public MidiEventArgs(MidiEvent midiEvent, IntPtr p1, IntPtr p2)
        {
            this.midiEvent = midiEvent;
            this.p1 = p1;
            this.p2 = p2;
        }

        public MidiEvent MidiEvent
        {
            get { return midiEvent; }
        }

        public IntPtr P1
        {
            get { return p1; }
        }

        public IntPtr P2
        {
            get { return p2; }
        }
    }
}
