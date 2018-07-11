//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MidiLibrary.CommonMessages;

namespace MidiLibrary.Instruments
{
    /// <summary>
    /// Immutable class representing a specific controller for an instrument
    /// </summary>
    public class Controller
    {
        #region Private members

        // Instrument this controller belongs to
        private Instrument instrument;

        // Name of the controller
        private string name;

        // Controller number
        private uint number;

        #endregion

        #region Constructor

        public Controller(Instrument instrument, string name, uint number)
        {
            this.instrument = instrument;
            this.name = name;
            this.number = number;
        }

        #endregion

        #region Properties

        public Instrument Instrument
        {
            get { return instrument; }
        }

        public string Name
        {
            get { return name; }
        }

        public uint Number
        {
            get { return number; }
        }

        public bool IsSwitch
        {
            get { return number >= 64 && number <= 69; }
        }

        /// <summary>
        /// Standard controller names (from VST3's ivstmidicontrollers.h)
        /// </summary>
        static public readonly string[] StandardControllerNames = new string[]
        {
            "  0: Bank Select MSB",
            "  1: Modulation Wheel",
            "  2: Breath",
            "  3: -",
            "  4: Foot Controller",
            "  5: Portamento Time",
            "  6: Data Entry MSB",
            "  7: Channel Volume",
            "  8: Balance",
            "  9: -",
            " 10: Pan",
            " 11: Expression",
            " 12: Effect Control 1",
            " 13: Effect Control 2",
            " 14: -",
            " 15: -",
            " 16: General Purpose 1",
            " 17: General Purpose 2",
            " 18: General Purpose 3",
            " 19: General Purpose 4",
            " 20: -",
            " 21: -",
            " 22: -",
            " 23: -",
            " 24: -",
            " 25: -",
            " 26: -",
            " 27: -",
            " 28: -",
            " 29: -",
            " 30: -",
            " 31: -",
            " 32: Bank Select LSB",
            " 33: -",
            " 34: -",
            " 35: -",
            " 36: -",
            " 37: -",
            " 38: Data Entry LSB",
            " 39: -",
            " 40: -",
            " 41: -",
            " 42: -",
            " 43: -",
            " 44: -",
            " 45: -",
            " 46: -",
            " 47: -",
            " 48: -",
            " 49: -",
            " 50: -",
            " 51: -",
            " 52: -",
            " 53: -",
            " 54: -",
            " 55: -",
            " 56: -",
            " 57: -",
            " 58: -",
            " 59: -",
            " 60: -",
            " 61: -",
            " 62: -",
            " 63: -",
            " 64: Sustain Pedal",
            " 65: Portamento",
            " 66: Sostenuto",
            " 67: Soft Pedal",
            " 68: Legato Footswitch",
            " 69: Hold 2",
            " 70: Sound Variation",
            " 71: Filter Cutoff",
            " 72: Release Time",
            " 73: Attack Time",
            " 74: Brightness",
            " 75: Decay Time",
            " 76: Vibrato Rate",
            " 77: Vibrato Depth",
            " 78: Vibrato Delay",
            " 79: -",
            " 80: General Purpose 5",
            " 81: General Purpose 6",
            " 82: General Purpose 7",
            " 83: General Purpose 8",
            " 84: Portamento Control",
            " 85: -",
            " 86: -",
            " 87: -",
            " 88: -",
            " 89: -",
            " 90: -",
            " 91: Reverb",
            " 92: Effect 2 Depth",
            " 93: Chorus",
            " 94: Delay/variation",
            " 95: Effect 5 Depth",
            " 96: Data Increment",
            " 97: Data Decrement",
            " 98: NRPN Select LSB",
            " 99: NRPN Select MSB",
            "100: RPN Select LSB",
            "101: RPN Select MSB",
            "102: -",
            "103: -",
            "104: -",
            "105: -",
            "106: -",
            "107: -",
            "108: -",
            "109: -",
            "110: -",
            "111: -",
            "112: -",
            "113: -",
            "114: -",
            "115: -",
            "116: -",
            "117: -",
            "118: -",
            "119: -",
            "120: All Sounds Off",
            "121: Reset All Controllers",
            "122: Local Control On/Off",
            "123: All Notes Off",
            "124: Omni Mode Off + All Notes Off",
            "125: Omni Mode On  + All Notes Off",
            "126: Poly Mode On/Off + All Sounds Off",
            "127: Poly Mode On"
        };

        #endregion

        #region Services

        public MidiMessage GetMessage(uint channel, uint value)
        {
            return new MidiControlChangeMessage(channel, number, value);
        }

        public override string ToString()
        {
            return "Controller " + number + ": " + name;
        }

        #endregion
    }
}
