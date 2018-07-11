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
    /// Class to hold a RPN or NRPN
    /// </summary>
    public class ProgramNumber
    {
        #region Private members

        // Instrument this [N]RPN belongs to
        private Instrument instrument;

        // Iw we are an RPN
        private bool registered;

        // Name of the [N]RPN
        private string name;

        // Number (on 14 bits) of the [N]RPN
        private uint number;

        #endregion

        #region Constructor

        public ProgramNumber(Instrument instrument, bool registered, string name, uint number)
        {
            this.instrument = instrument;
            this.registered = registered;
            this.name = name;
            this.number = number;
        }

        #endregion

        #region Properties

        public Instrument Instrument
        {
            get { return instrument; }
        }

        public bool Registered
        {
            get { return registered; }
        }

        public string Name
        {
            get { return name; }
        }

        public uint Number
        {
            get { return number; }
        }

        #endregion

        #region Services

        public MidiMessage[] GetSequence(uint channel, uint value)
        {
            var result = new MidiMessage[3];
            result[0] = new MidiControlChangeMessage(channel, registered ? EMidiController.RpnMSB: EMidiController.NrpnMSB, number / 128);
            result[1] = new MidiControlChangeMessage(channel, registered ? EMidiController.RpnLSB : EMidiController.NrpnLSB, number % 128);
            result[2] = new MidiControlChangeMessage(channel, EMidiController.DataEntryMSB, value);
            return result;
        }

        public MidiMessage[] GetSequence(uint channel, uint value, uint fineTune)
        {
            var result = new MidiMessage[4];
            result[0] = new MidiControlChangeMessage(channel, registered ? EMidiController.RpnMSB : EMidiController.NrpnMSB, number / 128);
            result[1] = new MidiControlChangeMessage(channel, registered ? EMidiController.RpnLSB : EMidiController.NrpnLSB, number % 128);
            result[2] = new MidiControlChangeMessage(channel, EMidiController.DataEntryMSB, value);
            result[3] = new MidiControlChangeMessage(channel, EMidiController.DataEntryLSB, fineTune);
            return result;
        }

        public override string ToString()
        {
            return (registered ? "RPN" : "NRPN") + " " + number + ": " + name;
        }

        #endregion
    }
}
