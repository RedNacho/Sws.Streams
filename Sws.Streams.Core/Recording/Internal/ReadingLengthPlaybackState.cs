using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common;

namespace Sws.Streams.Core.Recording.Internal
{

    internal class ReadingLengthPlaybackState : IPlaybackState
    {

        private readonly TimeSpan _timeOffset;

        public TimeSpan TimeOffset { get { return _timeOffset; } }

        private const int LengthLength = 4;

        private readonly byte[] _rawDataBuffer = new byte[LengthLength];

        private byte[] RawDataBuffer { get { return _rawDataBuffer; } }

        private int RawDataRead { get; set; }

        public ReadingLengthPlaybackState(TimeSpan timeOffset)
        {
            _timeOffset = timeOffset;
        }

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
                return new ReadingDataPlaybackState(TimeOffset, BitConverter.ToInt32(RawDataBuffer, 0));
            }

            return this;
        }

    }

}
