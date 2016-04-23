//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using MidiLibrary.CommonMessages;
using MidiLibrary.MetaMessages;
using MidiLibrary.SysexMessages;

namespace MidiLibrary.FileIO
{
    public static class MidiFileParser
    {
        #region Midi file parsing

        public static MidiSequence ParseMidiFile(string filename)
        {
            MidiFileFormatReader mffr = new MidiFileFormatReader(filename);

            using (mffr)
            {
                //
                // Verify magic number
                //
                string marker = mffr.ReadString(4);
                if (marker != MidiFileFormat.midiFileMarker)
                {
                    throw new FormatException(string.Format("Invalid marker for file {0}, expected {1} found {2}", filename, MidiFileFormat.midiFileMarker, marker));
                }

                // Verify header size
                uint headerLength = mffr.ReadUInt32();
                if (headerLength != MidiFileFormat.midiFileHeaderLength)
                {
                    throw new FormatException(string.Format("Invalid header length for file {0}, expected {1} found {2}", filename, MidiFileFormat.midiFileHeaderLength, headerLength));
                }

                //
                // Parse header
                //

                //
                // The 6 next bytes are 16-bits values, defining
                // the format, #tracks, and ppqn
                //
                uint midiVersion = mffr.ReadUInt16(); // Midi version (0, 1, 2)
                uint numberOfTracks = mffr.ReadUInt16(); // Number of tracks
                uint resolution = mffr.ReadUInt16(); // PPQN
                uint ppqn = 0;
                uint smpteTempo = 0;

                //
                // ppqn: Pulse per quarter note: The number of tics
                // per quarter notes. The length of a tic is the
                // tempo (length of a quarter note) divided by ppqn.
                //
                //
                // if ppqn is < 0, then it is NOT a ppqn, it is a
                // tempo in SMTPE format (??)
                //
                if (0 != (resolution & 0x8000))
                {
                    ppqn = 0;
                    smpteTempo = resolution & 0x7FFF;
                }
                else
                {
                    ppqn = resolution;
                    smpteTempo = 0;
                }

                //
                // Create the midi sequence
                //
                MidiSequence result = new MidiSequence(midiVersion, ppqn, smpteTempo, filename);

                //
                // Create and parse the tracks
                //
                for (uint t = 0; t < numberOfTracks; t++)
                {
                    result.Tracks.Add(ParseMidiTrack(mffr, t));
                }

                // Return the sequence
                return result;
            }
        }

        #endregion

        #region Track parsing

        private static MidiTrack ParseMidiTrack(MidiFileFormatReader mffr, uint trackNumber)
        {
            uint time = 0;
            uint runningStatus = 0;
            MidiTrack result = new MidiTrack();

            //
            // Verify magic number
            //
            string marker = mffr.ReadString(4);
            if (marker != MidiFileFormat.midiTrackMarker)
            {
                throw new FormatException(string.Format("Invalid track marker for file {0}, expected {1} found {2}", mffr.Filename, MidiFileFormat.midiTrackMarker, marker));
            }

            //
            // Get the length of the track (bytes)
            //
            uint length = mffr.ReadUInt32();

            //
            // Read all the events in the track
            //
            for (mffr.ResetBytesRead(); mffr.BytesRead < length; )
            {
                // Parse event
                MidiEvent midiEvent = ParseMidiEvent(mffr, trackNumber, time, runningStatus);

                // Adjust time
                time = midiEvent.TrackTime;

                // Running status
                runningStatus = midiEvent.Message.RunningStatus;

                // Add to list
                result.EventList.AddLast(midiEvent);
            }

            return result;
        }

        #endregion

        #region Event parsing

        private static MidiEvent ParseMidiEvent(MidiFileFormatReader mffr, uint trackNumber, uint time, uint runningStatus)
        {
            // Get the delta time (time from the previous event)
            uint trackDeltaTime = mffr.ReadVarLengthQuantity();

            // Parse the message
            MidiMessage message = ParseMidiMessage(mffr, runningStatus);

            return new MidiEvent(message, trackNumber, time + trackDeltaTime, trackDeltaTime);
        }

        #endregion

        #region Message parsing

        // Parse a midi message from a midi file
        private static MidiMessage ParseMidiMessage(MidiFileFormatReader mffr, uint runningStatus)
        {
            MidiMessage result = null;
            uint rawCommand;
            uint p1 = 0;
            bool alreadyReadP1 = false;

            // Read the command
            rawCommand = mffr.ReadUInt8();

            if ((rawCommand & 0x80) == 0x80)
            {
                // New running status
                runningStatus = rawCommand;
            }
            else
            {
                // What we just read is actually p1 of a common message
                alreadyReadP1 = true;
                p1 = rawCommand;
                rawCommand = runningStatus;
            }

            // Get channel
            uint channel = (rawCommand & 0x0f) + 1;
            EMidiCommand command = (EMidiCommand)(rawCommand & 0xF0);

            if (command == EMidiCommand.SystemMessageMask)
            {
                // System message
                channel = 0;
                command = (EMidiCommand)rawCommand;

                switch (command)
                {
                    // System messages - Exclusive
                    case EMidiCommand.SystemExclusive:
                        {
                            // Determine length of body + end-of-exclusive
                            uint length = mffr.ReadVarLengthQuantity();
                            byte[] sysexBody = new byte[length + 1];

                            // Read body
                            sysexBody[0] = (byte)EMidiCommand.SystemExclusive;
                            for (uint b = 0; b < length; b++)
                            {
                                sysexBody[b + 1] = mffr.ReadUInt8();
                            }

                            // Verify end-of-exclusive
                            if (sysexBody[length] != (byte)EMidiCommand.EndOfSystemExclusive)
                            {
                                throw new FormatException("Sysex of length " + length + " ending with " + sysexBody[length] + " instead of F7");
                            }

                            result = new MidiSysexMessage(sysexBody);
                            break;
                        }

                    // System messages - Common
                    case EMidiCommand.SongPosition:
                    case EMidiCommand.SongSelect:
                    case EMidiCommand.TuneRequest:

                    // System messages - Real time
                    case EMidiCommand.TimingClock:
                    case EMidiCommand.StartSequence:
                    case EMidiCommand.ContinueSequence:
                    case EMidiCommand.StopSequence:
                    case EMidiCommand.AutoSensing:
                        throw new FormatException(string.Format("Found event type {0} - not supported!", command));

                    // Meta event
                    case EMidiCommand.MetaEvent:
                        result = ParseMidiMetaMessage(mffr);
                        break;
                }
            }
            else
            {
                if (!alreadyReadP1)
                {
                    p1 = mffr.ReadUInt8();
                }

                // Channel message
                switch (command)
                {
                    case EMidiCommand.NoteOff:
                    case EMidiCommand.NoteOn:
                    case EMidiCommand.KeyAfterTouch:
                        result = new MidiNoteMessage(channel, command, p1, mffr.ReadUInt8());
                        break;
                    case EMidiCommand.ControlChange:
                        result = new MidiControlChangeMessage(channel, p1, mffr.ReadUInt8());
                        break;
                    case EMidiCommand.PatchChange:
                        result = new MidiPatchChangeMessage(channel, p1);
                        break;
                    case EMidiCommand.ChannelAfterTouch:
                        result = new MidiChannelAfterTouchMessage(channel, p1);
                        break;
                    case EMidiCommand.PitchWheelChange:
                        result = new MidiPitchChangeMessage(channel, p1 + ((uint)mffr.ReadUInt8() << 7));
                        break;
                }
            }

            result.RunningStatus = runningStatus;
            return result;
        }

        #endregion

        #region Meta message parsing

        static private MidiMetaMessage ParseMidiMetaMessage(MidiFileFormatReader mffr)
        {
            MidiMetaMessage result = null;

            // Get message type
            EMetaEventType type = (EMetaEventType)mffr.ReadUInt8();

            // Get body length
            uint length = mffr.ReadVarLengthQuantity();

            switch (type)
            {
                case EMetaEventType.TrackSequenceNumber:
                    result = new MidiMetaTrackSequenceNumberMessage(mffr.ReadUInt16());
                    break;

                case EMetaEventType.TextEvent:
                case EMetaEventType.Copyright:
                case EMetaEventType.SequenceTrackName:
                case EMetaEventType.TrackInstrumentName:
                case EMetaEventType.Lyric:
                case EMetaEventType.Marker:
                case EMetaEventType.CuePoint:
                case EMetaEventType.ProgramName:
                case EMetaEventType.DeviceName:
                case EMetaEventType.MidiChannel:
                    result = new MidiMetaTextMessage(type, mffr.ReadString(length));
                    break;

                case EMetaEventType.MidiPort:
                    result = new MidiMetaMidiPortMessage(mffr.ReadUInt8());
                    break;

                case EMetaEventType.EndTrack:
                    // No extra data for the end of track message
                    result = new MidiMetaMessage(type);
                    break;

                case EMetaEventType.SetTempo:
                    result = new MidiMetaTempoMessage(mffr.ReadUInt24());
                    break;

                case EMetaEventType.SmpteOffset:
                    break;

                case EMetaEventType.TimeSignature:
                    result = new MidiMetaTimeSignatureMessage(mffr.ReadUInt8(), mffr.ReadUInt8(), mffr.ReadUInt8(), mffr.ReadUInt8());
                    break;

                case EMetaEventType.KeySignature:
                    result = new MidiMetaKeySignatureMessage(mffr.ReadUInt8(), (mffr.ReadUInt8() != 0));
                    break;

                case EMetaEventType.SequencerSpecific:
                    // Read body
                    byte[] body = new byte[length];
                    for (int i = 0; i < length; i++)
                    {
                        body[i] = mffr.ReadUInt8();
                    }
                    result = new MidiMetaMessage(type, body);

                    break;

                default:
                    throw new FormatException(string.Format("Found unknown or unsupported metaevent code {0}", type));
            }

            return result;
        }

        #endregion
    }
}
