//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.CommonMessages
{
    /// <summary>
    /// Control change common message
    /// </summary>
    public class MidiControlChangeMessage : MidiCommonMessage
    {
        #region Private members

        private EMidiController controller;
        private uint value;

        #endregion

        #region Constructor

        public MidiControlChangeMessage(uint channel, uint controller, uint value)
            : this(channel, (EMidiController)controller, value)
        {
        }

        public MidiControlChangeMessage(uint channel, EMidiController controller, uint value)
            : base(EMidiCommand.ControlChange, channel)
        {
            this.controller = controller;
            this.value = value;
        }

        #endregion

        #region Properties

        public EMidiController Controller
        {
            get { return controller; }
        }

        public uint Value
        {
            get { return value; }
        }

        public bool On
        {
            get { return value != 0; }
        }

        #endregion

        #region Cloning

        public override MidiCommonMessage Clone(uint newChannel)
        {
            return new MidiControlChangeMessage(newChannel, (uint)controller, value);
        }

        #endregion

        #region Services

        public override byte[] GetAsByteArray()
        {
            return new byte[] { GetFirstByte(), (byte)controller, (byte)value };
        }

        public override uint GetAsShortMessage()
        {
            uint result = GetFirstByte();
            result |= (uint)controller << 8;
            result |= value << 16;

            return result;
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}: Set {1} to {2}", base.ToString(), controller, value);
        }

        #endregion
    }
}
