//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MidiLibrary.MetaMessages;

namespace MidiLibrary
{
    // A midi sequence (collection of tracks)
    public class MidiSequence
    {
        #region Constants

        private const int DEFAULT_MIDI_VERSION = 1;
        private const int DEFAULT_SMPTE_TEMPO = 0;

        #endregion

        #region Constructors

        public MidiSequence(uint ppqn)
            : this(DEFAULT_MIDI_VERSION, ppqn, DEFAULT_SMPTE_TEMPO, null)
        {
        }

        public MidiSequence(uint midiVersion, uint ppqn, uint smpteTempo)
            : this(midiVersion, ppqn, smpteTempo, null)
        {
        }

        public MidiSequence(uint midiVersion, uint ppqn, uint smpteTempo, string fileName)
        {
            MidiVersion = midiVersion;
            PPQN = ppqn;
            SMPTETempo = smpteTempo;
            Tracks = new List<MidiTrack>();
            FileName = fileName;
        }

        #endregion

        #region Properties

        // Filename (if applicable)
        public string FileName
        {
            get;
            private set;
        }

        // Midi version (from midi file)
        public uint MidiVersion
        {
            get;
            private set;
        }

        // Pulse per quarter note
        public uint PPQN
        {
            get;
            private set;
        }

        // SMPTE tempo
        public uint SMPTETempo
        {
            get;
            private set;
        }

        // Tracks
        public List<MidiTrack> Tracks
        {
            get;
            private set;
        }

        #endregion

        #region Infos

        public MidiEvent GetNextKeySignature(MidiEvent start)
        {
            return GetNextMessageOfType(start, typeof(MidiMetaKeySignatureMessage));
        }

        public MidiEvent GetNextTimeSignature(MidiEvent start)
        {
            return GetNextMessageOfType(start, typeof(MidiMetaTimeSignatureMessage));
        }

        public MidiEvent GetNextTempo(MidiEvent start)
        {
            return GetNextMessageOfType(start, typeof(MidiMetaTempoMessage));
        }

        // Get the nearest event of a specific type across all tracks
        public MidiEvent GetNextMessageOfType(MidiEvent start, Type type)
        {
            MidiEvent result = null;
            uint tstart = (start == null) ? 0 : start.TrackTime;
            uint tend = uint.MaxValue;

            foreach (MidiTrack track in Tracks)
            {
                MidiEvent evt = track.GetNextMessageOfType(type, tstart, tend);
                if (evt != null)
                {
                    if ((result == null) || (evt.Timestamp < result.Timestamp))
                    {
                        result = evt;
                        tend = result.TrackTime;
                    }
                }
            }

            return result;
        }

        // Get the total duration (duration of longest track)
        public uint GetDuration()
        {
            uint duration = 0;
            foreach (MidiTrack t in Tracks)
            {
                uint d = t.GetDuration();
                if (d > duration)
                {
                    duration = d;
                }
            }
            return duration;
        }

        #endregion

        #region Debug

        public void Dump()
        {
            Console.WriteLine("Midi version {0}, PPQN {1}, {2} tracks", MidiVersion, PPQN, Tracks.Count);
            for (int t = 0; t < Tracks.Count; t++)
            {
                Console.WriteLine("Track {0}:", t);
                Tracks[t].Dump();
            }
        }

        #endregion
    }
}
