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

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, Pitch {1}", base.ToString(), pitch);
        }

        #endregion
    }
}
