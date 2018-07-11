//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.MetaMessages
{
    /// <summary>
    /// Class to represent all midi meta messages storing text
    /// </summary>
    public class MidiMetaTextMessage : MidiMetaMessage
    {
        #region Private members

        string text;

        #endregion

        #region Constructor

        public MidiMetaTextMessage(EMetaEventType type, string text)
            : base(type)
        {
            this.text = text;
        }

        #endregion

        #region Properties

        public string Text
        {
            get { return text; }
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, Text {1}", base.ToString(), text);
        }

        #endregion
    }
}
