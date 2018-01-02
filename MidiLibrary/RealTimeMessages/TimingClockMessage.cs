//
// Copyright 2018 Benoit J. Merlet
//

namespace MidiLibrary.RealTimeMessages
{
    /// <summary>
    /// Class to hold a timing clock message (F8)
    /// </summary>
    public class TimingClockMessage : MidiMessage
    {
        #region Constructors

        public TimingClockMessage()
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
