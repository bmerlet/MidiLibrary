//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.MetaMessages
{
    /// <summary>
    /// Midi port meta midi message.
    /// </summary>
    public class MidiMetaMidiPortMessage : MidiMetaMessage
    {
        #region Private members

        private byte port;

        #endregion

        #region Constructor

        public MidiMetaMidiPortMessage(byte port)
            : base(EMetaEventType.MidiPort)
        {
            this.port = port;
        }

        #endregion

        #region Properties

        public uint Port
        {
            get { return port; }
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, Port {1}", base.ToString(), port);
        }

        #endregion
    }
}
