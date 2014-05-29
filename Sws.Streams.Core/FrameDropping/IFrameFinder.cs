using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.FrameDropping
{

    /// <summary>
    /// Scans Streams (which may be assumed to be sequentially readable) for frames.
    /// </summary>

    public interface IFrameFinder
    {

        /// <summary>
        /// Scans a given Stream for frames.  See FrameSearchResult for more information.
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        FrameSearchResult ScanForCompleteFrames(Stream sourceStream);
    }

}