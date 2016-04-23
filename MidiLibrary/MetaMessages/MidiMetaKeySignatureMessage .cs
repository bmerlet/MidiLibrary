//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.MetaMessages
{
    /// <summary>
    /// Key signature midi meta event
    /// </summary>
    public class MidiMetaKeySignatureMessage : MidiMetaMessage
    {
        #region Private members

        private sbyte sharpsFlats; // -7 -> 7 flats, 0 -> key of C, 7 ->7 sharps
        private bool minor;

        #endregion

        #region Constructor

        public MidiMetaKeySignatureMessage(byte sharpsFlats, bool minor)
            : base(EMetaEventType.KeySignature)
        {
            this.sharpsFlats = (sbyte)sharpsFlats;
            this.minor = minor;
        }

        #endregion

        #region Properties

        public int Sharps
        {
            get { return (sharpsFlats > 0) ? sharpsFlats : 0; }
        }

        public int Flats
        {
            get { return (sharpsFlats < 0) ? -sharpsFlats : 0; }
        }

        public sbyte SharpsFlat
        {
            get { return sharpsFlats; }
        }

        public bool Minor
        {
            get { return minor; }
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, #/b {1}, minor {2}", base.ToString(), sharpsFlats, minor);
        }

        #endregion
    }
}
