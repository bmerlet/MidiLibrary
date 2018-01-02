//
// Copyright 2018 Benoit J. Merlet
//

namespace MidiLibrary.RealTimeMessages
{
    /// <summary>
    /// Class to hold a start sequence message (FA)
    /// </summary>
    public class StartSequenceMessage : MidiMessage
    {
        #region Constructors

        public StartSequenceMessage()
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
