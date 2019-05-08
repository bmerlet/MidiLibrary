using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary
{
    internal class Platform
    {
        static private bool? runningOnMono;

        // Utility to check if running on Mono
        public static bool IsRunningOnMono
        {
            get
            {
                if (runningOnMono == null)
                {
                    runningOnMono = Type.GetType("Mono.Runtime") != null;
                }

                return runningOnMono == true;
            }
        }
    }
}
