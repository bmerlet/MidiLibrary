﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MidiLibrary.Alsa;
using MidiLibrary.WindowsMultiMedia;

namespace MidiLibrary.PortIO
{

    public class PortEnumerator
    {
        static public IMidiOutputPort[] OutputPorts
        {
            get
            {
                if (IsRunningOnMono())
                {
                    return AlsaMidiOutputPort.GetAllPorts();
                }
                else
                {
                    return WindowsMidiOutputPort.GetAllPorts();
                }
            }
        }

        static public IMidiInputPort[] InputPorts
        {
            get
            {
                if (IsRunningOnMono())
                {
                    //return AlsaMidiInputPort.GetAllPorts();
                    return new IMidiInputPort[0];
                }
                else
                {
                    return WindowsMidiInputPort.GetAllPorts();
                }
            }
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
