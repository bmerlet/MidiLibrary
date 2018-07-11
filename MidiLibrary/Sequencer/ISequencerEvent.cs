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
    // Interface that an event must satisfy to be played by the sequencer
    public interface ISequencerEvent
    {
        /// <summary>
        /// Time of the event in midi units
        /// </summary>
        uint TrackTime
        {
            get;
        }

        /// <summary>
        /// Message carried by this event
        /// </summary>
        ISequencerMessage SequencerMessage
        {
            get;
        }
    }
}
