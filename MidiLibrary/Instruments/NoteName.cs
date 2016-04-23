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
    /// Associate a name with a specific note of a patch
    /// </summary>
    public class NoteName
    {
        #region Private members

        // Patch this note name belongs to
        private Patch patch;

        // Name
        private string name;

        // Note number
        private uint number;

        #endregion

        #region Constructor

        public NoteName(Patch patch, string name, uint number)
        {
            this.patch = patch;
            this.name = name;
            this.number = number;
        }

        #endregion

        #region Properties

        public Patch Patch
        {
            get { return patch; }
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

        public override string ToString()
        {
            return "Note name " + number + ": " + name;
        }

        #endregion
    }
}
