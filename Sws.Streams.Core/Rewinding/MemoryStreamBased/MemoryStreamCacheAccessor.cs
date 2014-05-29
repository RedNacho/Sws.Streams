using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rewinding.MemoryStreamBased
{
    public class MemoryStreamCacheAccessor : ICacheAccessor
    {

        private MemoryStream _stream = new MemoryStream();

        public MemoryStream Stream { get { return _stream; } }

        public void ActOnCacheWriteStream(Action<Stream> action)
        {
            Stream.Position = Stream.Length;

            action(Stream);
        }

        public void ActOnCacheReadStream(Action<Stream> action, long distanceFromEnd)
        {
            Stream.Position = Stream.Length - distanceFromEnd;

            action(Stream);
        }

        public void TruncateCache(long tailLength)
        {
            tailLength = Math.Min(Stream.Length, tailLength);

            Stream.Position = Stream.Length - tailLength;

            var buffer = new byte[tailLength];

            if (tailLength > 0)
            {
                Stream.Read(buffer, 0, buffer.Length);
            }

            _stream = new MemoryStream();

            _stream.Write(buffer, 0, buffer.Length);

            _stream.Position = 0;
        }

        public void DestroyCache()
        {
            TruncateCache(0);
        }

    }
}
