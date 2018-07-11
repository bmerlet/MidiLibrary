//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Threading;
using MidiLibrary.MetaMessages;

namespace MidiLibrary.Sequencer
{
    /// <summary>
    /// Plays a sequence of event. The events must implement the ISequencerEvent interface.
    /// The sequencer does not play anything by itself; instead, when an event needs to be played,
    /// the OnSequencerEventPlayed event is raised.
    /// </summary>
    public class MidiSequencer : IDisposable
    {
        #region Private members

        private class CompareEvents : IComparer<ISequencerEvent>
        {
            public int Compare(ISequencerEvent x, ISequencerEvent y)
            {
                return (int)x.TrackTime - (int)y.TrackTime;
            }
        }

        //
        // General members
        //
        private uint ppqn;

        // The events to be played
        private ISequencerEvent[] events;

        // High resolution clock
        private HighResolutionClock hrc;

        // The player thread
        private Thread player;

        //
        // Members set by user and read by the player thread
        //
        // Set when disposing of the sequencer, makes the player thread exit.
        private bool closing;

        // Set on "play", cleared on "Pause"
        private bool playing;

        // Tempo, in microseconds per quarter note. Expected to be set by user when processing a tempo events
        private uint tempo;

        // Set by the user to request the player thread to position itself to a specific time.
        // The player thread sets this back to maxvalue when it is done.
        private uint positionToTime;

        // Lock object to send pulses to the player thread
        private object playerRequest;

        //
        // Members set by the player thread
        //
        // Set by the player when inside the playing loop
        private bool inPlayingLoop;

        // Set by the player when it stops playing, to indicate what tiem it stopped at.
        private uint timeWhenStopped;

        //
        // Members internal to the player thread
        //
        // Current index in the events array
        private int curIndex;

        // Midi time used as a reference
        private uint t0Midi;

        // High resolution time used as a reference
        private ulong t0Tick; // tick time used as reference
        
        #endregion

        #region Constructor

        public MidiSequencer(MidiSequence sequence)
            : this(GetEventArrayFromMidiSequence(sequence), sequence.PPQN)
        {
        }

        public MidiSequencer(ISequencerEvent[] events, uint ppqn)
        {
            this.curIndex = 0;
            this.ppqn = 960;
            this.tempo = 1000000; // One second per quarter note, 60bpm
            this.t0Midi = 0;
            this.t0Tick = 0;
            this.hrc = new HighResolutionClock();
            this.playing = false;
            this.closing = false;
            this.inPlayingLoop = false;
            this.positionToTime = 0;
            this.timeWhenStopped = 0;
            this.ppqn = ppqn;
            this.events = events;
            this.player = new Thread(new ThreadStart(Player));
            this.player.Priority = ThreadPriority.Highest;
            this.playerRequest = new object();
        }

        private static ISequencerEvent[] GetEventArrayFromMidiSequence(MidiSequence sequence)
        {
            List<ISequencerEvent> eventList = new List<ISequencerEvent>();

            // Gather all the events of the sequence in one list
            foreach (var t in sequence.Tracks)
            {
                eventList.AddRange(t.EventList);
            }

            // Morph the list into an array
            ISequencerEvent[] events = eventList.ToArray();

            // Sort the array
            Array.Sort(events, new CompareEvents());

            return events;
        }

        #endregion

        #region Event

        public delegate void SequencerEventHandler(object sender, SequencerEventArg e);
        public event SequencerEventHandler OnSequencerEventPlayed;

        #endregion

        #region Properties

        public uint Tempo
        {
            get { return tempo; }
            set { tempo = value; }
        }

        #endregion

        #region Controls

        // Start the playing thread, after listeners have registered
        public void StartPlayerThread()
        {
            positionToTime = 0;

            // Start thread
            player.Start();

            // Wait until thread has played events at time 0.
            while (positionToTime != uint.MaxValue)
            {
                Thread.Yield();
            }
        }

        public void UpdateSequence(ISequencerEvent[] events, uint ppqn)
        {
            // For now...
            if (playing)
            {
                throw new InvalidOperationException();
            }

            // Remember when we stopped
            uint tws = timeWhenStopped;

            // Update the events
            this.events = events;
            this.ppqn = ppqn;

            // Re-position
            PositionToTime(tws);
        }

        public void Play()
        {
            playing = true;
            lock (playerRequest)
            {
                Monitor.Pulse(playerRequest);
            }
        }

        public void Pause()
        {
            playing = false;
            while (inPlayingLoop)
            {
                Thread.Yield();
            }
        }

        public void PositionToTime(uint time)
        {
            if (playing)
            {
                throw new InvalidOperationException();
            }

            positionToTime = time;
            lock (playerRequest)
            {
                Monitor.Pulse(playerRequest);
            }

            while (positionToTime != uint.MaxValue)
            {
                Thread.Yield();
            }
        }

        #endregion

        #region Player thread

        private void Player()
        {
            uint time = 0;
            uint timeOfNextEvent = 0;
            bool firstTime = true;

            while (!closing)
            {
                // Wait to be told to do something
                if (!playing && !closing && !firstTime)
                {
                    lock (playerRequest)
                    {
                        Monitor.Wait(playerRequest);
                    }
                }

                firstTime = false;

                // Reset to specified point in time if requested
                if (positionToTime != uint.MaxValue)
                {
                    time = positionToTime;
                    timeOfNextEvent = PositionCurIndexToTime(positionToTime);
                    positionToTime = uint.MaxValue;

                    // Play all events at time 0 (tempo, key sig, ...)
                    if (time == 0)
                    {
                        timeOfNextEvent = PlayEventsAtCurrentTime(time);
                    }

                    continue;
                }

                // Remember current time
                SetupTimeReference(time);

                // Play!
                while (playing)
                {
                    inPlayingLoop = true;

                    // Compute current time (from start of play) in usecs
                    ulong now = hrc.GetElapsedInMicroSeconds(t0Tick);

                    // Compute time of next midi event in usecs
                    ulong timeOfNextEventInMicroSeconds = MidiTimeToMicroSeconds(timeOfNextEvent - t0Midi);

                    // Decide whether to wait, yield or play
                    ulong delta = (timeOfNextEventInMicroSeconds > now) ? (timeOfNextEventInMicroSeconds - now) : 0;
                    if (delta > 2000)
                    {
                        Thread.Sleep((int)delta / 2000);
                        continue;
                    }

                    if (delta > 0)
                    {
                        Thread.Yield();
                        continue;
                    }

                    // Compute new midi time
                    time = t0Midi + MicroSecondsToMidiTime(now);

                    // Play all events at current time
                    timeOfNextEvent = PlayEventsAtCurrentTime(time);
                }

                inPlayingLoop = false;
                timeWhenStopped = timeOfNextEvent;

                SequencerEventArg arg = new SequencerEventArg(SequencerEventArg.EType.Reset, null);
                OnSequencerEventPlayed(this, arg);
            }
        }

        // Postion to an arbitrary point in the sequence
        private uint PositionCurIndexToTime(uint targetTime)
        {
            uint timeOfFirstEventAfterTargetTime = uint.MaxValue;

            for (curIndex = 0; curIndex < events.Length; curIndex++)
            {
                // If we are at correct time, done
                if (events[curIndex].TrackTime >= targetTime)
                {
                    timeOfFirstEventAfterTargetTime = events[curIndex].TrackTime;
                    break;
                }
            }

            return timeOfFirstEventAfterTargetTime;
        }

        // Play all events at a given time, return time of closest next event
        private uint PlayEventsAtCurrentTime(uint time)
        {
            uint nextTime = uint.MaxValue;

            // Play all events at the current time
            for (; (curIndex < events.Length) && (events[curIndex].TrackTime <= time); curIndex++)
            {
                // Play this event!
                ISequencerMessage message = events[curIndex].SequencerMessage;

                // If this is a meta sequencer message, it affects the player itself
                if (message is ISequencerMetaMessage)
                {
                    // Process meta-messages locally
                    ApplyMetaMessage(events[curIndex], (ISequencerMetaMessage)message);
                }

                // Notify listeners
                if (OnSequencerEventPlayed != null)
                {
                    SequencerEventArg arg = new SequencerEventArg(SequencerEventArg.EType.Play, events[curIndex]);
                    OnSequencerEventPlayed(this, arg);
                }
            }

            // End of sequence
            if (curIndex == events.Length)
            {
                // Notify listeners
                if (OnSequencerEventPlayed != null)
                {
                    SequencerEventArg arg = new SequencerEventArg(SequencerEventArg.EType.End, null);
                    OnSequencerEventPlayed(this,arg);
                }

                playing = false;
                nextTime = 0;
            }
            else
            {
                nextTime = events[curIndex].TrackTime;
            }

            return nextTime;
        }

        // Parse and apply a meta message
        private void ApplyMetaMessage(ISequencerEvent evt, ISequencerMetaMessage message)
        {
            switch (message.SequencerType)
            {
                case ESequencerMessageType.Tempo:
                    this.tempo = message.SequencerTempo;
                    SetupTimeReference(evt.TrackTime);
                    break;
            }
        }

        #endregion

        #region Private methods

        // Convert midi time to microseconds
        private ulong MidiTimeToMicroSeconds(uint midiTime)
        {
            // Tempo: microseconds between quarter notes
            // PPQN: Pulse (ticks) per quarter note
            // We have Tempo / PPQN microseconds between ticks.
            return ((ulong)midiTime * (ulong)tempo) / (ulong)ppqn ;
        }

        // Convert microseconds to midi time
        private uint MicroSecondsToMidiTime(ulong microSecs)
        {
            return (uint)((microSecs * (ulong)ppqn) / tempo);
        }

        private void SetupTimeReference(uint midiTime)
        {
            t0Tick = hrc.Value;
            t0Midi = midiTime;
        }

        #endregion

        #region Destructor
        
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                closing = true;
                playing = false;
                lock (playerRequest)
                {
                    Monitor.Pulse(playerRequest);
                }
                if (player != null)
                {
                    player.Join();
                    player = null;
                }
            }
        }

        #endregion
    }
}
