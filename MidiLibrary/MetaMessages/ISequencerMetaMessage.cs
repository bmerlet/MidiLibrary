//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.MetaMessages
{
    public enum ESequencerMessageType { Tempo };

    /// <summary>
    /// Messages that affect the sequencer implement this interface
    /// </summary>
    public interface ISequencerMetaMessage
    {
        /// <summary>
        /// Return the type of event
        /// </summary>
        ESequencerMessageType SequencerType
        {
            get;
        }

        /// <summary>
        /// For Tempo messages: Tempo in microseconds per quarter note
        /// </summary>
        uint SequencerTempo
        {
            get;
        }
    }
}
