using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Rewinding;
using Sws.Streams.Core.FrameDropping;

namespace Sws.Streams.Extensions
{
    public static class RewindableExtensions
    {

        public static Stream ToFrameDroppingStream(this IRewindable rewindable, IFrameFinder frameFinder)
        {
            return ToFrameDroppingStreamBuilder(rewindable, frameFinder).Build();
        }

        public static FrameDroppingStreamBuilder ToFrameDroppingStreamBuilder(this IRewindable rewindable, IFrameFinder frameFinder)
        {
            return new FrameDroppingStreamBuilder(rewindable, frameFinder);
        }

    }
}
