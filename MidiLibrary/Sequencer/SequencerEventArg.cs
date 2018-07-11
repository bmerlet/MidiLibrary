//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.Sequencer
{
    /// <summary>
    /// Argument to the OnSequencerEventPlayed event
    /// </summary>
    public class SequencerEventArg : EventArgs
    {
        public enum EType { Play, Reset, End };

        internal SequencerEventArg(EType type, ISequencerEvent evt)
        {
            Type = type;
            Event = evt;
        }

        public EType Type
        {
            get;
            private set;
        }

        public ISequencerEvent Event
        {
            get;
            private set;
        }
    }
}
