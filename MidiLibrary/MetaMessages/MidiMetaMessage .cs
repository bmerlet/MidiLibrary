//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.MetaMessages
{
    // Base class for all meta-messages
    public class MidiMetaMessage : MidiMessage
    {
        #region Private members

        private EMetaEventType type;
        private byte[] body;

        #endregion

        #region Constructors

        public MidiMetaMessage(EMetaEventType type, byte[] body)
            : base(EMidiCommand.MetaEvent, 0)
        {
            this.type = type;
            this.body = body;
        }

        public MidiMetaMessage(EMetaEventType type)
            : this(type, null)
        {
        }

        #endregion

        #region Properties

        public EMetaEventType Type
        {
            get { return type; }
        }

        public byte[] Body
        {
            get { return body; }
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, Type {1}", base.ToString(), type);
        }

        #endregion
    }

}
