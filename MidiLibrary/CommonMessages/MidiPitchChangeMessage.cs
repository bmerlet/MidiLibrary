//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.CommonMessages
{
    /// <summary>
    /// Pitch change common message
    /// </summary>
    public class MidiPitchChangeMessage : MidiCommonMessage
    {
        #region Private members

        private int pitch;

        #endregion

        #region Constructor

        public MidiPitchChangeMessage(uint channel, uint rawChangeAmount)
            : base(EMidiCommand.PitchWheelChange, channel)
        {
            this.pitch = (int)rawChangeAmount - 0x2000;
        }

        #endregion

        #region Properties

        public int Pitch
        {
            get { return pitch; }
        }

        #endregion

        #region Cloning

        public override MidiCommonMessage Clone(uint newChannel)
        {
            return new MidiPitchChangeMessage(newChannel, (uint)pitch + 0x2000);
        }

        #endregion

        #region Services

        public override byte[] GetAsByteArray()
        {
            uint change = (uint)(pitch + 0x2000);
            return new byte[] { GetFirstByte(), (byte)(change & 0x7f), (byte)(change >> 7) };
        }

        public override uint GetAsShortMessage()
        {
            uint result = GetFirstByte();
            uint change = (uint)(pitch + 0x2000);
            result |= (change & 0x7f) << 8;
            result |= ((change >> 7) & 0x7f) << 16;

            return result;
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, Pitch {1}", base.ToString(), pitch);
        }

        #endregion
    }
}
