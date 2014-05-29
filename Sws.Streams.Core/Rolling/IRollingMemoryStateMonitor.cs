using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Rolling
{

    /// <summary>
    /// Receives notifications when data is written to or read from rolling memory.
    /// </summary>
    public interface IRollingMemoryStateMonitor
    {

        /// <summary>
        /// Records that the write position has advanced (ie data has been written to the rolling memory).
        /// </summary>
        /// <param name="delta">The amount that the write position has advanced.</param>
        void WritePositionAdvanced(long delta);

        /// <summary>
        /// Records that the read position has advanced (ie data has been read from the rolling memory).
        /// </summary>
        /// <param name="delta">The amount that the read position has advanced.</param>
        void ReadPositionAdvanced(long delta);
    }

}
