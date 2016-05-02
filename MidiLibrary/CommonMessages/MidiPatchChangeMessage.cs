//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.CommonMessages
{
    /// <summary>
    /// Midi patch change common message
    /// </summary>
    public class MidiPatchChangeMessage : MidiCommonMessage
    {
        #region Private members

        private uint patch;

        #endregion

        #region Constructor

        public MidiPatchChangeMessage(uint channel, uint patch)
            : base(EMidiCommand.PatchChange, channel)
        {
            this.patch = patch;
        }

        #endregion

        #region Properties

        public uint Patch
        {
            get { return patch; }
        }

        #endregion

        #region Cloning

        public override MidiCommonMessage Clone(uint newChannel)
        {
            return new MidiPatchChangeMessage(newChannel, patch);
        }

        #endregion

        #region Services

        public override byte[] GetAsByteArray()
        {
            return new byte[] { GetFirstByte(), (byte)patch };
        }

        public override uint GetAsShortMessage()
        {
            uint result = GetFirstByte();
            result |= patch << 8;

            return result;
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, Patch {1}", base.ToString(), patch);
        }

        #endregion
    }
}
