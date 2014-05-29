using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Rewinding;
using Sws.Streams.Core.FrameDropping;
using Sws.Streams.Core.Adapters;
using Sws.Streams.Core.Forwarding;

namespace Sws.Streams.Extensions
{

    public static class StreamExtensions
    {

        public static RewindableStream ToRewindableStream(this Stream stream)
        {
            return AdapterFactory.CreateRewindableStream(ToRewindable(stream), true);
        }

        public static IRewindable ToRewindable(this Stream stream)
        {
            return ToRewindableBuilder(stream).Build();
        }

        public static RewindableBuilder ToRewindableBuilder(this Stream stream)
        {
            return new RewindableBuilder(stream);
        }

        public static StreamForwarderBuilder ToStreamForwarderBuilder(this Stream stream, Stream targetStream, int bufferSize)
        {
            return new StreamForwarderBuilder(stream, targetStream, bufferSize);
        }

        public static IStreamForwarder ToStreamForwarder(this Stream stream, Stream targetStream, int bufferSize)
        {
            return ToStreamForwarderBuilder(stream, targetStream, bufferSize).Build();
        }

        public static void Forward(this Stream stream, Stream targetStream, int bufferSize)
        {
            ToStreamForwarder(stream, targetStream, bufferSize).Run();
        }

    }

}
