//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MidiLibrary.CommonMessages;
using MidiLibrary.SysexMessages;

namespace MidiLibrary.Instruments
{
    /// <summary>
    /// A set of midi messages (usually sysex) understood by a specific instrument
    /// </summary>
    public class Sequence
    {
        #region Private members

        // Group this sequence is part of
        private SequenceGroup sequenceGroup;

        // Name of the sequence
        private string name;

        // Sequence data in byte array form
        private byte[] data;

        // Sequence data in message form
        private MidiMessage[] messages;

        #endregion

        #region Constructor

        public Sequence(SequenceGroup sequenceGroup, string name, byte[] data)
        {
            this.sequenceGroup = sequenceGroup;
            this.name = name;
            this.data = data;
            this.messages = null;
        }

        // Parse sequence from ascii string
        public static Sequence BuildSequence(SequenceGroup sequenceGroup, string name, string byteStr)
        {
            var data = new List<byte>();

            // Parse ascii hex byte values
            var split = byteStr.Trim().Split(new char[] { ' ', '\t' });

            foreach (var bs in split)
            {
                data.Add(byte.Parse(bs, System.Globalization.NumberStyles.AllowHexSpecifier));
            }

            return new Sequence(sequenceGroup, name, data.ToArray());
        }

        #endregion

        #region Properties

        public SequenceGroup SequenceGroup
        {
            get { return sequenceGroup; }
        }

        public string Name
        {
            get { return name; }
        }

        public byte[] Data
        {
            get { return data; }
        }

        public MidiMessage[] MidiMessages
        {
            get { return GetMessages(); }
        }

        #endregion

        #region Message builder

        private MidiMessage[] GetMessages()
        {
            if (messages == null)
            {
                var msgList = new List<MidiMessage>();

                for (int b = 0; b < data.Length; b++)
                {
                    MidiMessage result = null;

                    uint rawCommand = data[b];
                    uint channel = (rawCommand & 0x0f) + 1;
                    EMidiCommand command = (EMidiCommand)(rawCommand & 0xF0);

                    if (command != EMidiCommand.SystemMessageMask)
                    {
                        // Common message
                        switch (command)
                        {
                            case EMidiCommand.NoteOff:
                            case EMidiCommand.NoteOn:
                            case EMidiCommand.KeyAfterTouch:
                                result = new MidiNoteMessage(channel, command, data[++b], data[++b]);
                                break;
                            case EMidiCommand.ControlChange:
                                result = new MidiControlChangeMessage(channel, data[++b], data[++b]);
                                break;
                            case EMidiCommand.PatchChange:
                                result = new MidiPatchChangeMessage(channel, data[++b]);
                                break;
                            case EMidiCommand.ChannelAfterTouch:
                                result = new MidiChannelAfterTouchMessage(channel, data[++b]);
                                break;
                            case EMidiCommand.PitchWheelChange:
                                result = new MidiPitchChangeMessage(channel, data[++b] + ((uint)data[++b] << 7));
                                break;
                            default:
                                throw new InvalidOperationException("Unsupported message in sequence: " + command);
                        }
                    }
                    else if (rawCommand == (uint)EMidiCommand.SystemExclusive)
                    {
                        var sysexBody = new List<byte>();

                        sysexBody.Add((byte)EMidiCommand.SystemExclusive);
                        byte bodyByte = 0;

                        while ((bodyByte = data[++b]) != (uint)EMidiCommand.EndOfSystemExclusive)
                        {
                            sysexBody.Add(bodyByte);
                        }

                        sysexBody.Add((byte)EMidiCommand.EndOfSystemExclusive);

                        result = new MidiSysexMessage(sysexBody.ToArray());
                    }
                    else
                    {
                         // System message
                        throw new InvalidOperationException("Only sysex and common messages supported in sequence");
                    }

                    msgList.Add(result);

                }
                
                messages = msgList.ToArray();
            }

            return messages;
        }

        #endregion
    }
}
