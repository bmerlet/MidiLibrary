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
    /// Represents a specific mid instrument, as read from a .INS file
    /// </summary>
    public class Instrument
    {
        #region Private members

        // Name of this instrument
        private string name;

        // How to select patches
        private EPatchSelectionMethod patchSelectionMethod;

        // All banks
        private List<Bank> banks;

        // All controllers
        private List<Controller> controllers;

        // All RPNs
        private List<ProgramNumber> rpns;

        // All NRPNs
        private List<ProgramNumber> nrpns;

        // All sequence groups
        private List<SequenceGroup> sequenceGroups;

        #endregion

        #region Constructor

        public Instrument(string name, EPatchSelectionMethod patchSelectionMethod)
        {
            this.name = name;
            this.patchSelectionMethod = patchSelectionMethod;
            this.banks = new List<Bank>();
            this.controllers = new List<Controller>();
            this.rpns = new List<ProgramNumber>();
            this.nrpns = new List<ProgramNumber>();
            this.sequenceGroups = new List<SequenceGroup>();
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
        }

        public EPatchSelectionMethod PatchSelectionMethod
        {
            get { return patchSelectionMethod; }
        }

        public List<Bank> Banks
        {
            get { return banks; }
        }

        public List<Controller> Controllers
        {
            get { return controllers; }
        }

        public List<ProgramNumber> RPNs
        {
            get { return rpns; }
        }

        public List<ProgramNumber> NRPNs
        {
            get { return nrpns; }
        }

        public List<SequenceGroup> SequenceGroups
        {
            get { return sequenceGroups; }
        }

        #endregion
    }
}
