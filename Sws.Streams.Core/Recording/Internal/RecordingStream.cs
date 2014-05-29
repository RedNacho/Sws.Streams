using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common;
using Sws.Streams.Core.Common.AbstractStreamImplementations;
using Sws.Streams.Core.Common.Internal;

namespace Sws.Streams.Core.Recording.Internal
{

    internal class RecordingStream : NonSeekableWriteOnlyStream
    {

        private readonly Stream _targetStream;

        public Stream TargetStream { get { return _targetStream; } }

        private readonly ICurrentDateTimeSource _currentDateTimeSource;

        public ICurrentDateTimeSource CurrentDateTimeSource { get { return _currentDateTimeSource; } }

        private readonly DateTime _constructionTimestamp;

        private DateTime ConstructionTimestamp { get { return _constructionTimestamp; } }

        private readonly object _writeSyncObject = new object();

        private object WriteSyncObject { get { return _writeSyncObject; } }

        public RecordingStream(Stream targetStream, ICurrentDateTimeSource currentDateTimeSource, DateTime constructionTimestamp)
        {
            if (targetStream == null)
                throw new ArgumentException("targetStream");

            if (currentDateTimeSource == null)
                throw new ArgumentNullException("currentDateTimeSource");

            _targetStream = targetStream;

            _currentDateTimeSource = currentDateTimeSource;

            _constructionTimestamp = constructionTimestamp;
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            lock (WriteSyncObject)
            {
                TargetStream.Flush();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
                throw new IndexOutOfRangeException(ExceptionMessages.OffsetPlusCountGreaterThanBufferSizeMessage);

            lock (WriteSyncObject)
            {

                long timeOffsetLong = CurrentDateTimeSource.GetCurrentDateTime().Subtract(ConstructionTimestamp).Ticks;

                var timeOffset = BitConverter.GetBytes(timeOffsetLong);

                TargetStream.Write(timeOffset, 0, timeOffset.Length);

                int lengthInt = count;

                var length = BitConverter.GetBytes(lengthInt);

                TargetStream.Write(length, 0, length.Length);

                TargetStream.Write(buffer, offset, count);

            }
        }

    }

}
