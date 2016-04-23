//
// Copyright 2016 Benoit J. Merlet
//

using System;
using System.Runtime.InteropServices;

namespace MidiLibrary.Sequencer
{
    /// <summary>
    /// Interface to the processor tick counter
    /// </summary>
    public class HighResolutionClock
    {
        #region Private fields

        private ulong frequency = 0;

        #endregion

        #region Constructor

        public HighResolutionClock()
        {
            // Query the high-resolution timer only if it is supported.
            // A returned frequency of 1000 typically indicates that it is not
            // supported and is emulated by the OS using the same value that is
            // returned by Environment.TickCount.
            // A return value of 0 indicates that the performance counter is
            // not supported.
            int returnVal = NativeMethods.QueryPerformanceFrequency(ref frequency);

            if (returnVal == 0 || frequency == 1000)
            {
                // The performance counter is not supported.
                frequency = 0;
                throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Return the frequency of the clock (in Hz)
        /// </summary>
        public ulong Frequency
        {
            get
            {
                return frequency;
            }
        }

        /// <summary>
        /// Return the current clock value (in ticks)
        /// </summary>
        public ulong Value
        {
            get
            {
                ulong tickCount = 0;
                NativeMethods.QueryPerformanceCounter(ref tickCount);
                return tickCount;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Reutn the number of ticks elapsed since the specified time
        /// </summary>
        /// <param name="t0">Start time, in tick</param>
        /// <returns>Elapsed time, in ticks</returns>
        public ulong GetElapsedInTicks(ulong t0)
        {
            return Value - t0;
        }

        /// <summary>
        /// Return the number of microseconds elapsed since the specified time
        /// </summary>
        /// <param name="t0">Start time, in ticks</param>
        /// <returns>Elapsed time, in microseconds</returns>
        public uint GetElapsedInMicroSeconds(ulong t0)
        {
            return (uint)TicksToMicroseconds(Value - t0);
        }

        /// <summary>
        /// Convert a number of ticks to microseconds
        /// </summary>
        /// <param name="ticks">Number of ticks</param>
        /// <returns>Number of microseconds</returns>
        public ulong TicksToMicroseconds(ulong ticks)
        {
            ulong timeElapseInMicroSeconds = (ticks * (ulong)1000000) / frequency;
            return timeElapseInMicroSeconds;
        }

        #endregion

        #region Native methods

        private class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern int QueryPerformanceCounter(ref ulong count);
            [DllImport("kernel32.dll")]
            public static extern int QueryPerformanceFrequency(ref ulong frequency);
        }

        #endregion
    }
}
