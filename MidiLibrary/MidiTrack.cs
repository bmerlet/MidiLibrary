//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;

namespace MidiLibrary
{
    public class MidiTrack
    {
        #region Private members

        private LinkedList<MidiEvent> eventList;

        #endregion

        #region Constructor

        public MidiTrack()
        {
            eventList = new LinkedList<MidiEvent>();
        }

        #endregion

        #region Properties

        // Events making up this track
        public LinkedList<MidiEvent> EventList
        {
            get { return eventList; }
        }

        #endregion

        #region Actions

        // Build delta times, assuming that the absolute times are set.
        public void BuildDeltaTimes()
        {
            uint lastTime = 0;
            for (LinkedListNode<MidiEvent> n = eventList.First; n != null; n = n.Next)
            {
                // Check track is ordered...
                if (lastTime > n.Value.TrackTime)
                {
                    throw new InvalidOperationException();
                }

                n.Value.TrackDeltaTime = n.Value.TrackTime - lastTime;
                lastTime = n.Value.TrackTime;
            }
        }

        #endregion

        #region Info

        public MidiEvent GetNextMessageOfType(Type type, uint tstart, uint tend)
        {
            MidiEvent result = null;

            foreach (MidiEvent evt in eventList)
            {
                if (evt.TrackTime < tstart)
                {
                    continue;
                }

                if (evt.TrackTime >= tend)
                {
                    break;
                }

                if (evt.Message.GetType() == type)
                {
                    result = evt;
                    break;
                }
            }
            return result;
        }

        public uint GetDuration()
        {
            return eventList.Last.Value.TrackTime;
        }

        #endregion

        #region Debug

        // Dump the track to stdout
        public void Dump()
        {
            var e = eventList.GetEnumerator();

            while (e.MoveNext())
            {
                Console.WriteLine("  Event {0}", e.Current);
            }
        }

        #endregion
    }
}
