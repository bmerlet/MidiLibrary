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
