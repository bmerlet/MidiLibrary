//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.MetaMessages
{
    /// <summary>
    /// Set tempo midi meta message
    /// </summary>
    public class MidiMetaTempoMessage : MidiMetaMessage, ISequencerMetaMessage
    {
        #region Private members

        uint tempo;

        #endregion

        #region Constructor

        public MidiMetaTempoMessage(uint tempo)
            : base(EMetaEventType.SetTempo)
        {
            this.tempo = tempo;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Type of event, for the sequencer
        /// </summary>
        public ESequencerMessageType SequencerType
        {
            get { return ESequencerMessageType.Tempo; }
        }

        /// <summary>
        /// Returns tempo in microseconds per quarter note
        /// </summary>
        public uint Tempo
        {
            get { return tempo; }
        }

        /// <summary>
        /// Returns tempo in microseconds per quarter note (sequencer interface)
        /// </summary>
        public uint SequencerTempo
        {
            get { return tempo; }
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, Tempo {1}", base.ToString(), tempo);
        }

        #endregion
    }
}
