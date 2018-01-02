//
// Copyright 2018 Benoit J. Merlet
//

namespace MidiLibrary.RealTimeMessages
{
    /// <summary>
    /// Class to hold a stop sequence message (FC)
    /// </summary>
    public class StopSequenceMessage : MidiMessage
    {
        #region Constructors

        public StopSequenceMessage()
            : base(EMidiCommand.TimingClock)
        {
        }

        #endregion

        #region Services

        public override byte[] GetAsByteArray()
        {
            return new byte[] { GetFirstByte() };
        }

        public override uint GetAsShortMessage()
        {
            return GetFirstByte();
        }

        #endregion
    }
}
