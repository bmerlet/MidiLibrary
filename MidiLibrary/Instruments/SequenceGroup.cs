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
    /// A group of sequences, all related to the same function of an instrument
    /// </summary>
    public class SequenceGroup
    {
        #region Private members

        // The instrument this sequence group is part of
        private Instrument instrument;

        // Name of this sequence group
        private string name;

        // Number of this sequence group (not used)
        private uint number;

        // Sequences belonging to this group
        private List<Sequence> sequences;

        #endregion

        #region Constructor

        public SequenceGroup(Instrument instrument, string name, uint number)
        {
            this.instrument = instrument;
            this.name = name;
            this.number = number;
            this.sequences = new List<Sequence>();
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

        public List<Sequence> Sequences
        {
            get { return sequences; }
        }

        #endregion
    }
}
