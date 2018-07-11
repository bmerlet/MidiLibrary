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
        #region Constructor

        public Instrument(string name, EPatchSelectionMethod patchSelectionMethod, EOverlapSupport overlapSupport, bool supportsChannelAfterTouch, bool supportsPitchBend, string vstiName)
        {
            this.Name = name;
            this.PatchSelectionMethod = patchSelectionMethod;
            this.SupportsChannelAfterTouch = supportsChannelAfterTouch;
            this.SupportsPitchBend = supportsPitchBend;
            this.OverlapSupport = overlapSupport;
            this.VstiName = vstiName;
            this.Banks = new List<Bank>();
            this.Controllers = new List<Controller>();
            this.RPNs = new List<ProgramNumber>();
            this.NRPNs = new List<ProgramNumber>();
            this.SequenceGroups = new List<SequenceGroup>();
        }

        #endregion

        #region Properties

        // Name of this instrument
        public readonly string Name;

        // How to select patches
        public readonly EPatchSelectionMethod PatchSelectionMethod;

        // Overlap support
        public readonly EOverlapSupport OverlapSupport;

        // If supports channel after touch
        public readonly bool SupportsChannelAfterTouch;

        // If supports pitch bend
        public readonly bool SupportsPitchBend;

        // If this instrument is for a specific VSTI, name of the VSTI. Null otherwise.
        public readonly string VstiName;

        // All banks
        public readonly List<Bank> Banks;

        // All controllers
        public readonly List<Controller> Controllers;

        // All RPNs
        public readonly List<ProgramNumber> RPNs;

        // All NRPNs
        public readonly List<ProgramNumber> NRPNs;

        // All sequence groups
        public readonly List<SequenceGroup> SequenceGroups;

        #endregion
    }
}
