//
// Copyright 2016 Benoit J. Merlet
//

using System;
using MidiLibrary.Sequencer;

namespace MidiLibrary
{
    /// <summary>
    /// MidiMessage: Base class for all midi messages
    /// </summary>
    public abstract class MidiMessage : ISequencerMessage
    {
        #region Private members

        private EMidiCommand command;
        private uint channel;
        private uint runningStatus;

        #endregion

        #region Constructors

        protected MidiMessage(EMidiCommand command, uint channel)
        {
            this.command = command;
            this.channel = channel;
            this.runningStatus = 0;
        }

        protected MidiMessage(EMidiCommand command)
            : this(command, 0)
        {
        }

        #endregion

        #region Properties

        // Midi command
        public EMidiCommand Command
        {
            get { return command; }
        }

        // Channel (1-16) for channel messages, 0 otherwise
        public uint Channel
        {
            get { return channel; }
        }

        // Running status (when parsing from file)
        public uint RunningStatus
        {
            get { return runningStatus; }
            set { runningStatus = value; }
        }

        #endregion

        #region Services

        /// <summary>
        /// Get the midi message as an array of byte, as they would be sent over a midi cable
        /// </summary>
        /// <returns>Midi message as byte array</returns>
        public abstract byte[] GetAsByteArray();

        /// <summary>
        /// Get the midi message as an unsigned integer, in the windows short message format
        /// </summary>
        /// <returns>Windows format short message, of uint.MaxValue if not supported by the message type</returns>
        public abstract uint GetAsShortMessage();

        /// <summary>
        /// Get the first byte of the midi message, as it would appear on the wire
        /// </summary>
        /// <returns>First byte of midi message</returns>
        public byte GetFirstByte()
        {
            return (byte)((byte)command | ((channel == 0) ? 0 : channel - 1));
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0} ", command);
        }

        #endregion

    }
}
