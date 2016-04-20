//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MidiLibrary.CommonMessages;
using MidiLibrary.MetaMessages;
using MidiLibrary.SysexMessages;

namespace MidiLibrary.FileIO
{
    public static class MidiFileWriter
    {
        #region Midi file writing

        public static void WriteMidiFile(MidiSequence sequence, string filename)
        {
            MidiFileFormatWriter mffw = new MidiFileFormatWriter(filename);

            using (mffw)
            {
                // Write magic number
                mffw.WriteString(MidiFileFormat.midiFileMarker, false);

                // Write header size
                mffw.WriteUInt32(MidiFileFormat.midiFileHeaderLength);

                // Write header
                mffw.WriteUInt16(MidiFileFormat.midiFileVersion);
                mffw.WriteUInt16((ushort)sequence.Tracks.Count);
                mffw.WriteUInt16((ushort)((sequence.PPQN > 0) ? sequence.PPQN : (sequence.SMPTETempo | 0x8000)));

                // Write out the tracks
                for (int t = 0; t < sequence.Tracks.Count; t++)
                {
                    WriteMidiTrack(mffw, sequence.Tracks[t]);
                }
            }
        }

        #endregion

        #region Track writing

        private static void WriteMidiTrack(MidiFileFormatWriter mffw, MidiTrack track)
        {
            // Write out track marker
            mffw.WriteString(MidiFileFormat.midiTrackMarker, false);

            // Run 2 passes, one to compute the length to be written, and one to actually write out the data
            mffw.CountingMode = true;
            mffw.ResetBytesWritten();

            for (int pass = 0; pass < 2; pass++)
            {
                byte runningStatus = 0;

                // Write out all events
                for (LinkedListNode<MidiEvent> n = track.EventList.First; n != null; n = n.Next)
                {
                    runningStatus = WriteMidiEvent(mffw, n.Value, runningStatus);
                }

                // At the end of the first pass, write out the count
                if (pass == 0)
                {
                    mffw.CountingMode = false;
                    uint count = mffw.BytesWritten;
                    mffw.WriteUInt32(count);
                }
            }
        }

        #endregion

        #region Event writing

        private static byte WriteMidiEvent(MidiFileFormatWriter mffw, MidiEvent evt, byte runningStatus)
        {
            // Write out delta time
            mffw.WriteVarLengthQuantity(evt.TrackDeltaTime);

            // Write out the message
            return WriteMidiMessage(mffw, evt.Message, runningStatus);
        }

        #endregion

        #region Message writing

        // Write a midi message to a midi file
        private static byte WriteMidiMessage(MidiFileFormatWriter mffw, MidiMessage message, byte runningStatus)

        {
            // Write out the command unless it is the same as the running status
            byte firstByte = (byte)((uint)message.Command | (uint)message.Channel);

            if (firstByte != runningStatus)
            {
                mffw.WriteUInt8(firstByte);
                runningStatus = firstByte;
            }

            // Act according to message type
            if (message is MidiSysexMessage)
            {
                runningStatus = 0;
                byte[] body = ((MidiSysexMessage)message).Body;

                // Write out length (-1 to skip F0)
                mffw.WriteVarLengthQuantity((uint)body.Length - 1);

                // Write out body
                for (int b = 1; b < (body.Length - 1); b++)
                {
                    if ((body[b] & 0x80) != 0)
                    {
                        throw new ArgumentException("WriteMidiMessage: Write Sysex: high bit set in body");
                    }

                    mffw.WriteUInt8(body[b]);
                }

                // Write out end of sysex
                mffw.WriteUInt8((byte)EMidiCommand.EndOfSystemExclusive);
            }
            else if (message is MidiMetaMessage)
            {
                runningStatus = 0;
                WriteMidiMetaMessage(mffw, message as MidiMetaMessage);
            }
            else if (message is MidiNoteMessage)
            {
                var note = message as MidiNoteMessage;
                mffw.WriteUInt8((byte)note.Note);
                mffw.WriteUInt8((byte)note.Velocity);
            }
            else if (message is MidiControlChangeMessage)
            {
                var cc = message as MidiControlChangeMessage;
                mffw.WriteUInt8((byte)cc.Controller);
                mffw.WriteUInt8((byte)cc.Value);
            }
            else if (message is MidiPatchChangeMessage)
            {
                var pc = message as MidiPatchChangeMessage;
                mffw.WriteUInt8((byte)pc.Patch);
            }
            else if (message is MidiChannelAfterTouchMessage)
            {
                var cat = message as MidiChannelAfterTouchMessage;
                mffw.WriteUInt8((byte)cat.Velocity);
            }
            else if (message is MidiPitchChangeMessage)
            {
                var pc = message as MidiPitchChangeMessage;
                uint pitch = (uint)(pc.Pitch + 0x2000);
                mffw.WriteUInt8((byte)(pitch & 0x7f));
                mffw.WriteUInt8((byte)((pitch >> 7) & 0x7f));
            }
            else
            {
                throw new FormatException(string.Format("Found event type {0} - not supported!", message.GetType().FullName));
            }

            return runningStatus;
        }

        #endregion

        #region Meta message writing

        static private void WriteMidiMetaMessage(MidiFileFormatWriter mffw, MidiMetaMessage message)
        {
            // Write out message type
            mffw.WriteUInt8((byte)message.Type);

            // Write out message content
            if (message is MidiMetaTrackSequenceNumberMessage)
            {
                var m = message as MidiMetaTrackSequenceNumberMessage;
                mffw.WriteVarLengthQuantity(2);
                mffw.WriteUInt16((ushort)m.SequenceNumber);
            }
            else if (message is MidiMetaTextMessage)
            {
                var m = message as MidiMetaTextMessage;
                mffw.WriteString(m.Text, true);
            }
            else if (message is MidiMetaMidiPortMessage)
            {
                var m = message as MidiMetaMidiPortMessage;
                mffw.WriteVarLengthQuantity(1);
                mffw.WriteUInt8((byte)m.Port);
            }
            else if (message is MidiMetaTempoMessage)
            {
                var m = message as MidiMetaTempoMessage;
                mffw.WriteVarLengthQuantity(3);
                mffw.WriteUInt24(m.Tempo);
            }
            else if (message is MidiMetaTimeSignatureMessage)
            {
                var m = message as MidiMetaTimeSignatureMessage;
                mffw.WriteVarLengthQuantity(4);
                mffw.WriteUInt8((byte)m.Numerator);
                mffw.WriteUInt8((byte)m.Denominator);
                mffw.WriteUInt8((byte)m.TicksInMetronomeClick);
                mffw.WriteUInt8((byte)m.No32ndNotesInQuarterNote);
            }
            else if (message is MidiMetaKeySignatureMessage)
            {
                var m = message as MidiMetaKeySignatureMessage;
                mffw.WriteVarLengthQuantity(2);
                mffw.WriteUInt8((byte)((m.Sharps > 0) ? m.Sharps : -m.Flats));
                mffw.WriteUInt8((byte)(m.Minor ? 1 : 0));
            }
            else if (message.Type == EMetaEventType.SequencerSpecific)
            {
                mffw.WriteVarLengthQuantity((uint)message.Body.Length);
                for (int b = 0; b < message.Body.Length; b++)
                {
                    mffw.WriteUInt8(message.Body[b]);
                }
            }
            else if (message.Type == EMetaEventType.EndTrack)
            {
                mffw.WriteVarLengthQuantity(0);
            }
            else
            {
                throw new FormatException(string.Format("Found event type {0} - not supported!", message.GetType().FullName));
            }
        }

        #endregion
    }
}
