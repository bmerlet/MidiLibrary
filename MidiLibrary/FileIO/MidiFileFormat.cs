//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.FileIO
{
    static class MidiFileFormat
    {
        // Midi file version we are using
        public const ushort midiFileVersion = 1;

        // Midi file starts with the string:
        public const string midiFileMarker = "MThd";

        // It is then followed by a 32-bit quantity indicating the header length which must be:
        public const uint midiFileHeaderLength = 6;

        // Each track starts with the string:
        public const string midiTrackMarker = "MTrk";
    }
}
