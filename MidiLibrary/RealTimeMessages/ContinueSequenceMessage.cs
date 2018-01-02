//
// Copyright 2018 Benoit J. Merlet
//

namespace MidiLibrary.RealTimeMessages
{
    /// <summary>
    /// Class to hold a continue sequence message (FB)
    /// </summary>
    public class ContinueSequenceMessage : MidiMessage
    {
        #region Constructors

        public ContinueSequenceMessage()
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
