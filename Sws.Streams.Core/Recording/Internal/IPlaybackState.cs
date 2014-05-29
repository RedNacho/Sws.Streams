using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common;

namespace Sws.Streams.Core.Recording.Internal
{
    internal interface IPlaybackState
    {
        int Read(Stream sourceStream, TimeSpan timeOffset, byte[] buffer, int offset, int count);

        IPlaybackState StateTransition();
    }
}
