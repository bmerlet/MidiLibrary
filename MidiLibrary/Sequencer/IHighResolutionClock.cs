//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiLibrary.Sequencer
{
    /// <summary>
    /// Interface to a high resolution clock
    /// </summary>
    public interface IHighResolutionClock
    {
        /// <summary>
        /// Get current time in ticks
        /// </summary>
        ulong CurrentTimeInTicks { get; }

        /// <summary>
        /// Return the number of microseconds elapsed since the specified time
        /// </summary>
        /// <param name="t0">Start time, in ticks</param>
        /// <returns>Elapsed time, in microseconds</returns>
        uint GetElapsedInMicroSeconds(ulong fromTimeInTicks);
    }

    /// <summary>
    /// Factory of high resolution clocks
    /// </summary>
    public static class HighResolutionClockFactory
    {
        public static IHighResolutionClock GetHighResolutionClock()
        {
            if (Platform.IsRunningOnMono)
            {
                return new GenericHighResolutionClock();
            }
            else
            {
                return new WindowsHighResolutionClock();
            }
        }
    }
}
