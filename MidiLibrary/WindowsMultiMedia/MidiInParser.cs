//
// Copyright 2016 Benoit J. Merlet
//

using MidiLibrary.CommonMessages;
using MidiLibrary.RealTimeMessages;

namespace MidiLibrary.WindowsMultiMedia
{
    public static class MidiInParser
    {
        #region Parsing MIM_DATA message (midi input)

        // Parse a midi message from a midi input MIM_DATA message
        public static MidiEvent ParseMimDataMessage(uint p1, uint p2)
        {
            MidiMessage message = ParseShortMessage(p1);
            return new MidiEvent(message, p2);
        }

        #endregion

        #region Create midi message from MIM_DATA message

        // Parse a midi message from a midi input MIM_DATA message
        private static MidiMessage ParseShortMessage(uint message)
        {
            MidiMessage result = null;
            uint rawCommand = message & 0xff;
            uint channel = (rawCommand & 0x0f) + 1;
            EMidiCommand command = (EMidiCommand)(rawCommand & 0xF0);

            if (command == EMidiCommand.SystemMessageMask)
            {
                // System message
                channel = 0;
                command = (EMidiCommand)rawCommand;

                switch (command)
                {
                    case EMidiCommand.TimingClock:
                        result = new TimingClockMessage();
                        break;
                    case EMidiCommand.StartSequence:
                        result = new StartSequenceMessage();
                        break;
                    case EMidiCommand.ContinueSequence:
                        result = new ContinueSequenceMessage();
                        break;
                    case EMidiCommand.StopSequence:
                        result = new StopSequenceMessage();
                        break;
                    case EMidiCommand.ActiveSensing:
                        result = new ActiveSensingMessage();
                        break;
                }
            }
            else
            {
                uint p1 = (message >> 8) & 0xff;
                uint p2 = (message >> 16) & 0xff;

                // Common message
                switch (command)
                {
                    case EMidiCommand.NoteOff:
                    case EMidiCommand.NoteOn:
                    case EMidiCommand.KeyAfterTouch:
                        result = new MidiNoteMessage(channel, command, p1, p2);
                        break;
                    case EMidiCommand.ControlChange:
                        result = new MidiControlChangeMessage(channel, p1, p2);
                        break;
                    case EMidiCommand.PatchChange:
                        result = new MidiPatchChangeMessage(channel, p1);
                        break;
                    case EMidiCommand.ChannelAfterTouch:
                        result = new MidiChannelAfterTouchMessage(channel, p1);
                        break;
                    case EMidiCommand.PitchWheelChange:
                        result = new MidiPitchChangeMessage(channel, p1 + (p2 << 7));
                        break;
                }
            }

            return result;
        }

        #endregion
    }
}
