using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.FrameDropping
{

    public class FrameDescription
    {

        /// <summary>
        /// The relative position of the frame.  This will be combined with the FinalRelativePosition of the FrameSearchResult
        /// to navigate to the frame.  For example, if the FinalRelativePosition = 5 and RelativePosition = 3, this means that 
        /// the search read two additional bytes after reaching the start of this frame, and that therefore to find the frame
        /// you would have to backtrack two bytes along the Stream.
        /// </summary>
        public long RelativePosition { get; set; }

        /// <summary>
        /// The length of the frame.
        /// </summary>
        public long FrameLength { get; set; }

    }

}
