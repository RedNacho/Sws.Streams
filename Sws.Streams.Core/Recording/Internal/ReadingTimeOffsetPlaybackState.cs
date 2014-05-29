using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common;

namespace Sws.Streams.Core.Recording.Internal
{
    internal class ReadingTimeOffsetPlaybackState : IPlaybackState
    {

        private const int TimeOffsetLength = 8;

        private readonly byte[] _rawDataBuffer = new byte[TimeOffsetLength];

        private byte[] RawDataBuffer { get { return _rawDataBuffer; } }

        private int RawDataRead { get; set; }

        public int Read(Stream sourceStream, TimeSpan timeOffset, byte[] buffer, int offset, int count)
        {
            if (RawDataRead < RawDataBuffer.Length)
            {
                RawDataRead += sourceStream.Read(RawDataBuffer, RawDataRead, RawDataBuffer.Length - RawDataRead);
            }

            return 0;
        }

        public IPlaybackState StateTransition()
        {
            if (RawDataRead >= RawDataBuffer.Length)
            {
                return new ReadingLengthPlaybackState(new TimeSpan(BitConverter.ToInt64(RawDataBuffer, 0)));
            }

            return this;
        }
    }
}
