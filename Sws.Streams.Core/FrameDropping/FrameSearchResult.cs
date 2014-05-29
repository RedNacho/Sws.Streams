using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.FrameDropping
{

    /// <summary>
    /// Represents the results of a frame search.
    /// </summary>
    public class FrameSearchResult
    {

        /// <summary>
        /// This is the relative position of the Stream when the frame search completed.  This will be
        /// combined with the RelativePositions in the FrameDescriptions to navigate back to the start
        /// of a frame.  The value itself is unimportant as long as the differences between it and
        /// the other RelativePositions is correct.
        /// </summary>
        public long FinalRelativePosition { get; set; }

        /// <summary>
        /// Frames found by the search.
        /// </summary>
        public IEnumerable<FrameDescription> FramesFound { get; set; }

    }
}
