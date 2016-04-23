//
// Copyright 2016 Benoit J. Merlet
//

using System;
using MidiLibrary.Sequencer;

namespace MidiLibrary
{
    // Generic delegate to handle a midi message
    public delegate void MidiMessageHandler(MidiMessage midiMessage);

    /// <summary>
    /// MidiMessage: Base class for all midi messages
    /// </summary>
    public class MidiMessage : ISequencerMessage
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

        #region Debug

        public override string ToString()
        {
            return String.Format("{0} ", command);
        }

        #endregion

    }
}
