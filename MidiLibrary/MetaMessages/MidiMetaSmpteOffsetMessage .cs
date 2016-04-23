//
// Copyright 2016 Benoit J. Merlet
//


namespace MidiLibrary.MetaMessages
{
    /// <summary>
    /// SMPTE offset midi meta message
    /// </summary>
    public class MidiMetaSmpteOffsetMessage : MidiMetaMessage
    {
        #region Private members

        private byte hours;
        private byte minutes;
        private byte seconds;
        private byte frames;
        private byte subFrames; // 100ths of a frame

        #endregion

        #region Constructors

        public MidiMetaSmpteOffsetMessage(byte hours, byte minutes, byte seconds, byte frames, byte subFrames)
            : base(EMetaEventType.SmpteOffset)
        {
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
            this.frames = frames;
            this.subFrames = subFrames;
        }

        #endregion

        #region Properties

        public uint Hours
        {
            get { return hours; }
        }

        public uint Minutes
        {
            get { return minutes; }
        }

        public uint Seconds
        {
            get { return seconds; }
        }

        public uint Frames
        {
            get { return frames; }
        }

        public uint SubFrames
        {
            get { return subFrames; }
        }

        #endregion
    }
}
