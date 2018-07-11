//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.Instruments
{
    /// <summary>
    /// Immutable class representing a bank of a midi instrument
    /// </summary>
    public class Bank
    {
        #region Private members

        // Instrument this bank belongs to
        private Instrument instrument;

        // Name of this bank
        private string name;

        // Number of this bank
        private uint number;

        // If this is a drum bank
        private bool drums;

        // List of patches in this bank
        private List<Patch> patches;

        #endregion

        #region Constructor

        public Bank(Instrument instrument, string name, uint number, bool drums)
        {
            this.instrument = instrument;
            this.name = name;
            this.drums = drums;
            this.number = number;
            this.patches = new List<Patch>();
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

        public bool Drums
        {
            get { return drums; }
        }

        public List<Patch> Patches
        {
            get { return patches; }
        }

        #endregion

        #region Services

        public override string ToString()
        {
            return "Bank " + number + ": " + name;
        }

        #endregion
    }
}
