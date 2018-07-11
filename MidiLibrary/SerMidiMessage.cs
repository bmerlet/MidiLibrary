//
// Copyright 2016 Benoit J. Merlet
//

using MidiLibrary.CommonMessages;
using MidiLibrary.SysexMessages;
using MidiLibrary.MetaMessages;
using System;
using System.Xml.Serialization;

namespace MidiLibrary
{
    #region Main class

    /// <summary>
    /// This class represent a midi message in a serializable form
    /// </summary>
    [XmlInclude(typeof(SerMidiChannelAfterTouchMessage))]
    [XmlInclude(typeof(SerMidiControlChangeMessage))]
    [XmlInclude(typeof(SerMidiNoteMessage))]
    [XmlInclude(typeof(SerMidiPatchChangeMessage))]
    [XmlInclude(typeof(SerMidiPitchChangeMessage))]
    [XmlInclude(typeof(SerMidiSysexMessage))]
    [XmlInclude(typeof(SerMidiMetaMidiPortMessage))]
    [XmlInclude(typeof(SerMidiMetaSmpteOffsetMessage))]
    [XmlInclude(typeof(SerMidiMetaTempoMessage))]
    [XmlInclude(typeof(SerMidiMetaTextMessage))]
    [XmlInclude(typeof(SerMidiMetaTimeSignatureMessage))]
    [XmlInclude(typeof(SerMidiMetaTrackSequenceNumberMessage))]
    [Serializable]
    public abstract class SerMidiMessage
    {
        public EMidiCommand command;
        public uint channel;

        // Default constructor, mandatory for serialization
        public SerMidiMessage()
        {
        }

        // Build a serializable midi message from an actual midi message
        protected SerMidiMessage(MidiMessage midiMessage)
        {
            this.command = midiMessage.Command;
            this.channel = midiMessage.Channel;
        }

        // Get a serializable representation of a midi message
        static public SerMidiMessage GetSerMidiMessage(MidiMessage src)
        {
            SerMidiMessage msg = null;

            if (src is MidiChannelAfterTouchMessage)
            {
                msg = new SerMidiChannelAfterTouchMessage((MidiChannelAfterTouchMessage)src);
            }
            else if (src is MidiControlChangeMessage)
            {
                msg = new SerMidiControlChangeMessage((MidiControlChangeMessage)src);
            }
            else if (src is MidiNoteMessage)
            {
                msg = new SerMidiNoteMessage((MidiNoteMessage)src);
            }
            else if (src is MidiPatchChangeMessage)
            {
                msg = new SerMidiPatchChangeMessage((MidiPatchChangeMessage)src);
            }
            else if (src is MidiPitchChangeMessage)
            {
                msg = new SerMidiPitchChangeMessage((MidiPitchChangeMessage)src);
            }
            else if (src is MidiSysexMessage)
            {
                msg = new SerMidiSysexMessage((MidiSysexMessage)src);
            }
            else if (src is MidiMetaKeySignatureMessage)
            {
                msg = new SerMidiMetaKeySignatureMessage((MidiMetaKeySignatureMessage)src);
            }
            else if (src is MidiMetaMidiPortMessage)
            {
                msg = new SerMidiMetaMidiPortMessage((MidiMetaMidiPortMessage)src);
            }
            else if (src is MidiMetaSmpteOffsetMessage)
            {
                msg = new SerMidiMetaSmpteOffsetMessage((MidiMetaSmpteOffsetMessage)src);
            }
            else if (src is MidiMetaTempoMessage)
            {
                msg = new SerMidiMetaTempoMessage((MidiMetaTempoMessage)src);
            }
            else if (src is MidiMetaTextMessage)
            {
                msg = new SerMidiMetaTextMessage((MidiMetaTextMessage)src);
            }
            else if (src is MidiMetaTimeSignatureMessage)
            {
                msg = new SerMidiMetaTimeSignatureMessage((MidiMetaTimeSignatureMessage)src);
            }
            else if (src is MidiMetaTrackSequenceNumberMessage)
            {
                msg = new SerMidiMetaTrackSequenceNumberMessage((MidiMetaTrackSequenceNumberMessage)src);
            }

            return msg;
        }

        // Get a midi message from its serializable representation
        abstract public MidiMessage GetMidiMessage();
    }

    #endregion

    #region Common messages

    [Serializable]
    public class SerMidiChannelAfterTouchMessage : SerMidiMessage
    {
        public uint velocity;

        public SerMidiChannelAfterTouchMessage()
        {
        }

        public SerMidiChannelAfterTouchMessage(MidiChannelAfterTouchMessage msg)
            : base(msg)
        {
            this.velocity = msg.Velocity;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiChannelAfterTouchMessage(channel, velocity);
        }
    }

    [Serializable]
    public class SerMidiControlChangeMessage : SerMidiMessage
    {
        public uint controller;
        public uint value;

        public SerMidiControlChangeMessage()
        {
        }

        public SerMidiControlChangeMessage(MidiControlChangeMessage msg)
            : base(msg)
        {
            this.controller = (uint)msg.Controller;
            this.value = msg.Value;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiControlChangeMessage(channel, controller, value);
        }
    }

    [Serializable]
    public class SerMidiNoteMessage : SerMidiMessage
    {
        public uint note;
        public uint velocity;

        public SerMidiNoteMessage()
        {
        }

        public SerMidiNoteMessage(MidiNoteMessage msg)
            : base(msg)
        {
            this.note = msg.Note;
            this.velocity = msg.Velocity;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiNoteMessage(channel, command, note, velocity);
        }
    }

    [Serializable]
    public class SerMidiPatchChangeMessage : SerMidiMessage
    {
        public uint patch;

        public SerMidiPatchChangeMessage()
        {
        }

        public SerMidiPatchChangeMessage(MidiPatchChangeMessage msg)
            : base(msg)
        {
            this.patch = msg.Patch;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiPatchChangeMessage(channel, patch);
        }
    }

    [Serializable]
    public class SerMidiPitchChangeMessage : SerMidiMessage
    {
        public int pitch;

        public SerMidiPitchChangeMessage()
        {
        }

        public SerMidiPitchChangeMessage(MidiPitchChangeMessage msg)
            : base(msg)
        {
            this.pitch = msg.Pitch;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiPitchChangeMessage(channel, (uint)(pitch + 0x2000));
        }
    }

    #endregion

    #region Sysex messages

    [Serializable]
    public class SerMidiSysexMessage : SerMidiMessage
    {
        public byte[] body;

        public SerMidiSysexMessage()
        {
        }

        public SerMidiSysexMessage(MidiSysexMessage msg)
            : base(msg)
        {
            this.body = msg.Body;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiSysexMessage(body);
        }
    }

    #endregion

    #region Meta messages

    [Serializable]
    public class SerMidiMetaKeySignatureMessage : SerMidiMessage
    {
        public sbyte sharpsFlats;
        public bool minor;

        public SerMidiMetaKeySignatureMessage()
        {
        }

        public SerMidiMetaKeySignatureMessage(MidiMetaKeySignatureMessage msg)
            : base(msg)
        {
            this.sharpsFlats = msg.SharpsFlat;
            this.minor = msg.Minor;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiMetaKeySignatureMessage((byte)sharpsFlats, minor);
        }
    }

    [Serializable]
    public class SerMidiMetaMidiPortMessage : SerMidiMessage
    {
        public byte port;

        public SerMidiMetaMidiPortMessage()
        {
        }

        public SerMidiMetaMidiPortMessage(MidiMetaMidiPortMessage msg)
            : base(msg)
        {
            this.port = (byte)msg.Port;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiMetaMidiPortMessage(port);
        }
    }

    [Serializable]
    public class SerMidiMetaSmpteOffsetMessage : SerMidiMessage
    {
        public byte hours;
        public byte minutes;
        public byte seconds;
        public byte frames;
        public byte subFrames;

        public SerMidiMetaSmpteOffsetMessage()
        {
        }

        public SerMidiMetaSmpteOffsetMessage(MidiMetaSmpteOffsetMessage msg)
            : base(msg)
        {
            this.hours = (byte)msg.Hours;
            this.minutes = (byte)msg.Minutes;
            this.seconds = (byte)msg.Seconds;
            this.frames = (byte)msg.Frames;
            this.subFrames = (byte)msg.SubFrames;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiMetaSmpteOffsetMessage(hours, minutes, seconds, frames, subFrames);
        }
    }

    [Serializable]
    public class SerMidiMetaTempoMessage : SerMidiMessage
    {
        public uint tempo;

        public SerMidiMetaTempoMessage()
        {
        }

        public SerMidiMetaTempoMessage(MidiMetaTempoMessage msg)
            : base(msg)
        {
            this.tempo = msg.Tempo;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiMetaTempoMessage(tempo);
        }
    }

    [Serializable]
    public class SerMidiMetaTextMessage : SerMidiMessage
    {
        public EMetaEventType type;
        public string text;

        public SerMidiMetaTextMessage()
        {
        }

        public SerMidiMetaTextMessage(MidiMetaTextMessage msg)
            : base(msg)
        {
            this.type = msg.Type;
            this.text = msg.Text;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiMetaTextMessage(type, text);
        }
    }

    [Serializable]
    public class SerMidiMetaTimeSignatureMessage : SerMidiMessage
    {
        public byte numerator;
        public byte denominator;
        public byte ticksInMetronomeClick;
        public byte no32ndNotesInQuarterNote;

        public SerMidiMetaTimeSignatureMessage()
        {
        }

        public SerMidiMetaTimeSignatureMessage(MidiMetaTimeSignatureMessage msg)
            : base(msg)
        {
            this.numerator = (byte)msg.Numerator;
            this.denominator = (byte)msg.Denominator;
            this.ticksInMetronomeClick = (byte)msg.TicksInMetronomeClick;
            this.no32ndNotesInQuarterNote = (byte)msg.No32ndNotesInQuarterNote;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiMetaTimeSignatureMessage(numerator, denominator, ticksInMetronomeClick, no32ndNotesInQuarterNote);
        }
    }

    [Serializable]
    public class SerMidiMetaTrackSequenceNumberMessage : SerMidiMessage
    {
        public uint sequenceNumber;

        public SerMidiMetaTrackSequenceNumberMessage()
        {
        }

        public SerMidiMetaTrackSequenceNumberMessage(MidiMetaTrackSequenceNumberMessage msg)
            : base(msg)
        {
            this.sequenceNumber = msg.SequenceNumber;
        }

        public override MidiMessage GetMidiMessage()
        {
            return new MidiMetaTrackSequenceNumberMessage(sequenceNumber);
        }
    }

    #endregion
}
