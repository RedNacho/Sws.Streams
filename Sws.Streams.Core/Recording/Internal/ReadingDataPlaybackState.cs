using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common;

namespace Sws.Streams.Core.Recording.Internal
{

    internal class ReadingDataPlaybackState : IPlaybackState
    {

        private readonly TimeSpan _timeOffset;

        public TimeSpan TimeOffset { get { return _timeOffset; } }

        private readonly int _length;

        public int Length { get { return _length; } }

        private int DataRead { get; set; }

        public ReadingDataPlaybackState(TimeSpan timeOffset, int length)
        {
            _timeOffset = timeOffset;
            _length = length;
        }

        public int Read(Stream sourceStream, TimeSpan timeOffset, byte[] buffer, int offset, int count)
        {
            int toRead = 0;

            int read = 0;

            if (timeOffset >= TimeOffset)
            {
                toRead = Math.Min(count, Length - DataRead);
            }

            if (toRead > 0)
            {
                read = sourceStream.Read(buffer, offset, toRead);
            }

            DataRead += read;

            return read;
        }

        public IPlaybackState StateTransition()
        {
            if (DataRead >= Length)
            {
                return new ReadingTimeOffsetPlaybackState();
            }

            return this;
        }
    }

}
