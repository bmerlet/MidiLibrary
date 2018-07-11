//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.CommonMessages
{
    /// <summary>
    /// Base class for all midi common messages
    /// </summary>
    public abstract class MidiCommonMessage : MidiMessage
    {
        public MidiCommonMessage(EMidiCommand command, uint channel) :
            base(command, channel)
        {
        }

        // Clone a common message to a new channel
        public abstract MidiCommonMessage Clone(uint newChannel);
    }
}
