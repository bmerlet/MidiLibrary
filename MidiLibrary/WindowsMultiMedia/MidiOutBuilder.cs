//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MidiLibrary.CommonMessages;

namespace MidiLibrary.WindowsMultiMedia
{
    static class MidiOutBuilder
    {
        // Build a window multimedia one-word message based on a MidiMessage
        public static uint BuildShortMessage(MidiMessage msg)
        {
            // The LSB of the message has the command and the channel
            uint command = (uint)msg.Command | ((msg.Channel == 0) ? 0 : (msg.Channel - 1));
            uint p1 = 0;
            uint p2 = 0;

            // 
            if (msg.GetType() == typeof(MidiNoteMessage))
            {
                p1 = ((MidiNoteMessage)msg).Note;
                p2 = ((MidiNoteMessage)msg).Velocity;
            }
            else if (msg.GetType() == typeof(MidiControlChangeMessage))
            {
                p1 = (uint)((MidiControlChangeMessage)msg).Controller;
                p2 = ((MidiControlChangeMessage)msg).Value;
            }
            else if (msg.GetType() == typeof(MidiPatchChangeMessage))
            {
                p1 = ((MidiPatchChangeMessage)msg).Patch;
            }
            else if (msg.GetType() == typeof(MidiChannelAfterTouchMessage))
            {
                p1 = ((MidiChannelAfterTouchMessage)msg).Velocity;
            }
            else if (msg.GetType() == typeof(MidiPitchChangeMessage))
            {
                uint pitch = (uint)((MidiPitchChangeMessage)msg).Pitch;
                p1 = pitch & 0x7f;
                p2 = (pitch >> 7) & 0x7f;
            }

            return (p2 << 16) | (p1 << 8) | command;
        }
    }
}
