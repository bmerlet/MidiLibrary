//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.MetaMessages
{
    /// <summary>
    /// Track sequence number midi meta-event
    /// </summary>
    public class MidiMetaTrackSequenceNumberMessage : MidiMetaMessage
    {
        #region Private members

        uint sequenceNumber;

        #endregion

        #region Constructor

        public MidiMetaTrackSequenceNumberMessage(uint sequenceNumber)
            : base(EMetaEventType.TrackSequenceNumber)
        {
            this.sequenceNumber = sequenceNumber;
        }

        #endregion

        #region Properties

        public uint SequenceNumber
        {
            get { return sequenceNumber; }
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, SeqNum {1}", base.ToString(), sequenceNumber);
        }

        #endregion
    }
}
