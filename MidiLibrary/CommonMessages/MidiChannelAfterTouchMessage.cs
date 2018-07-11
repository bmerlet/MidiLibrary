//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.CommonMessages
{
    /// <summary>
    /// After-touch midi common message. This is the "channel pressure" message, which applies to all active notes.
    /// </summary>
    public class MidiChannelAfterTouchMessage : MidiCommonMessage
    {
        #region Private members

        private uint velocity;

        #endregion

        #region Constructor

        public MidiChannelAfterTouchMessage(uint channel, uint velocity)
            : base(EMidiCommand.ChannelAfterTouch, channel)
        {
            this.velocity = velocity;
        }

        #endregion

        #region Properties

        public uint Velocity
        {
            get { return velocity; }
        }

        #endregion

        #region Cloning

        public override MidiCommonMessage Clone(uint newChannel)
        {
            return new MidiChannelAfterTouchMessage(newChannel, velocity);
        }

        #endregion

        #region Services

        public override byte[] GetAsByteArray()
        {
            return new byte[] { GetFirstByte(), (byte)velocity };
        }

        public override uint GetAsShortMessage()
        {
            uint result = GetFirstByte();
            result |= velocity << 8;

            return result;
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, Vel {1}", base.ToString(), velocity);
        }

        #endregion
    }
}
