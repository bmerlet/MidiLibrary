//
// Copyright 2016 Benoit J. Merlet
//


namespace MidiLibrary.SysexMessages
{
    /// <summary>
    /// Rudimentary class to hold a midi sysex message
    /// </summary>
    public class MidiSysexMessage : MidiMessage
    {
        #region Private members
        
        // Bytes of the sysex, including the starting F0 and the ending F7
        private byte[] body;

        #endregion

        #region Constructors

        public MidiSysexMessage(byte[] body)
            : base(EMidiCommand.SystemExclusive, 0)
        {
            this.body = body;
        }

        #endregion

        #region Properties

        public byte[] Body
        {
            get { return body; }
        }

        #endregion
    }
}
