//
// Copyright 2016 Benoit J. Merlet
//

using System;

namespace MidiLibrary.CommonMessages
{
    /// <summary>
    /// Class representing a note on, note off, or note aftertouch
    /// </summary>
    public class MidiNoteMessage : MidiCommonMessage
    {
        #region Private members

        private EMidiNoteAction action;
        private uint note;
        private uint velocity;

        #endregion

        #region Constructor

        public MidiNoteMessage(uint channel, EMidiCommand command, uint note, uint velocity)
            : base(command, channel)
        {
            switch (command)
            {
                case EMidiCommand.NoteOn:
                    action = (velocity == 0) ? EMidiNoteAction.Off : EMidiNoteAction.On;
                    break;
                case EMidiCommand.NoteOff:
                    action = EMidiNoteAction.Off;
                    break;
                case EMidiCommand.KeyAfterTouch:
                    action = EMidiNoteAction.VelChange;
                    break;
            }
            this.note = note;
            this.velocity = velocity;
        }

        #endregion

        #region Properties

        public EMidiNoteAction Action
        {
            get { return action; }
        }

        public uint Note
        {
            get { return note; }
        }

        public uint Velocity
        {
            get { return velocity; }
        }

        public double Level
        {
            get { return (double)velocity / 127.0; }
        }

        #endregion

        #region Cloning

        public override MidiCommonMessage Clone(uint newChannel)
        {
            return new MidiNoteMessage(newChannel, Command, note, velocity);
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("{0}, Note {1}, Vel {2}", base.ToString(), note, velocity);
        }

        #endregion

    }
}
