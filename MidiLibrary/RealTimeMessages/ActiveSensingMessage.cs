//
// Copyright 2018 Benoit J. Merlet
//

namespace MidiLibrary.RealTimeMessages
{
    /// <summary>
    /// Class to hold a active sensing message (FE)
    /// </summary>
    public class ActiveSensingMessage : MidiMessage
    {
        #region Constructors

        public ActiveSensingMessage()
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
