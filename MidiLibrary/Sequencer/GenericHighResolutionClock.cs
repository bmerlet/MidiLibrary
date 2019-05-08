using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.Sequencer
{
    /// <summary>
    /// High resolution clock not based on platform-dependant stuff
    /// </summary>
    public class GenericHighResolutionClock : IHighResolutionClock
    {
        private Stopwatch stopwatch;

        public GenericHighResolutionClock()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public ulong CurrentTimeInTicks => (ulong)stopwatch.ElapsedTicks;

        public uint GetElapsedInMicroSeconds(ulong fromTimeInTicks)
        {
            long elapsedInTicks = stopwatch.ElapsedTicks - (long)fromTimeInTicks;
            if (elapsedInTicks <= 0)
            {
                return 0;
            }

            long elapsedInMicroseconds = elapsedInTicks * 1000000 / Stopwatch.Frequency;
            return (uint)elapsedInMicroseconds;
        }
    }
}
