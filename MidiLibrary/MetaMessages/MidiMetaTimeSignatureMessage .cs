//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.MetaMessages
{
    /// <summary>
    /// Time signature (meter) midi meta message
    /// </summary>
    public class MidiMetaTimeSignatureMessage : MidiMetaMessage
    {
        #region Private members

        private byte numerator;
        private byte denominator; // 1 = half note, 2 = quarter note, 3 = eigth note etc
        private byte ticksInMetronomeClick;
        private byte no32ndNotesInQuarterNote;

        #endregion

        #region Constructor

        public MidiMetaTimeSignatureMessage(byte numerator, byte denominator, byte ticksInMetronomeClick, byte no32ndNotesInQuarterNote)
            : base(EMetaEventType.TimeSignature)
        {
            this.numerator = numerator;
            this.denominator = denominator;
            this.ticksInMetronomeClick = ticksInMetronomeClick;
            this.no32ndNotesInQuarterNote = no32ndNotesInQuarterNote;
        }

        #endregion

        #region Properties

        public uint Numerator
        {
            get { return numerator; }
        }

        public uint Denominator
        {
            get { return denominator; }
        }

        public uint TicksInMetronomeClick
        {
            get { return ticksInMetronomeClick; }
        }

        public uint No32ndNotesInQuarterNote
        {
            get { return no32ndNotesInQuarterNote; }
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, num {1}, denom {2}", base.ToString(), numerator, denominator);
        }

        #endregion
    }
}
