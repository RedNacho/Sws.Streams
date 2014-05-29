using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rewinding
{

    /// <summary>
    /// Provides access to a rewindable data source.
    /// </summary>
    public interface IRewindable : IDisposable
    {

        /// <summary>
        /// The data which can be rewound.
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// Rewinds the data position by the supplied number of bytes.  Next time you perform a Read on the Stream, this number of bytes will
        /// be repeated.
        /// </summary>
        /// <param name="step"></param>
        void Rewind(long step);

        /// <summary>
        /// Truncates the amount of data available for rewinding.  Rewinding from the current position any further than maximumRewindStep
        /// will not be supported.
        /// </summary>
        /// <param name="maximumRewindStep"></param>
        void Truncate(long maximumRewindStep);

        /// <summary>
        /// This event is raised when the position moves forwards (ie the Stream is read).
        /// </summary>
        event PositionChangedEventHandler PositionIncremented;

        /// <summary>
        /// This event is raised when the position moves backwards (ie a call to Rewind).
        /// </summary>
        event PositionChangedEventHandler PositionDecremented;
    }
}
