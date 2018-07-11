//
// Copyright 2016 Benoit J. Merlet
//

using System;
using MidiLibrary.Sequencer;

namespace MidiLibrary
{
    /// <summary>
    /// Class representing a midi event, i.e. a midi message happening at a specified time
    /// </summary>
    public class MidiEvent : ISequencerEvent
    {
        #region Private members

        private MidiMessage message;
        private uint timestamp; // When received via midi-in, time (in ms) since midiInStart()
        private uint trackTime; // When read from a midi file, time (in tics) since the beginning of the track 
        private uint trackDeltaTime; // When read from a midi file, time (in tics) since the previous MidiEvent in the track
        private uint track;    // Track this event belongs to (when applicable)

        #endregion

        #region Constructors

        // Constructor when reading from midi input
        internal MidiEvent(MidiMessage message, uint timestamp)
        {
            this.message = message;
            this.timestamp = timestamp;
            this.trackTime = 0;
            this.trackDeltaTime = 0;
            this.track = 0;
        }

        // Constructor when reading from midi file
        internal MidiEvent(MidiMessage message, uint track,  uint trackTime, uint trackDeltaTime)
        {
            this.message = message;
            this.timestamp = 0;
            this.track = track;
            this.trackTime = trackTime;
            this.trackDeltaTime = trackDeltaTime;
        }

        // Constructor when building a sequence manually
        public MidiEvent(MidiMessage message, uint track, uint trackTime)
        {
            this.message = message;
            this.timestamp = 0;
            this.track = track;
            this.trackTime = trackTime;
            this.trackDeltaTime = uint.MaxValue;
        }

        #endregion

        #region Properties

        // The midi message
        public MidiMessage Message
        {
            get { return message; }
        }

        // For the sequencer, midi message
        public ISequencerMessage SequencerMessage
        {
            get { return message; }
        }

        // Timestamp (when read from midi in)
        public uint Timestamp
        {
            get { return timestamp; }
        }

        // Time (in midi units) since beginning of track
        public uint TrackTime
        {
            get { return trackTime; }
        }

        // Time (in midi unit) since previous event in same track
        public uint TrackDeltaTime
        {
            get { return trackDeltaTime; }
            internal set { trackDeltaTime = value; }
        }

        // Track this event belongs to
        public uint Track
        {
            get { return track; }
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return String.Format("Time {0:D8}: {1}", trackTime, message);
        }

        #endregion
    }
}
