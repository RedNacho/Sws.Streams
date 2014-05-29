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

    internal class PlaybackStream : NonSeekableReadOnlyStream
    {

        private readonly Stream _sourceStream;

        public Stream SourceStream { get { return _sourceStream; } }

        private readonly ICurrentDateTimeSource _currentDateTimeSource;

        public ICurrentDateTimeSource CurrentDateTimeSource { get { return _currentDateTimeSource; } }

        private IPlaybackState CurrentPlaybackState { get; set; }

        private readonly DateTime _constructionTimestamp;

        private DateTime ConstructionTimestamp { get { return _constructionTimestamp; } }

        private readonly object _readSyncObject = new object();

        private object ReadSyncObject { get { return _readSyncObject; } }

        public PlaybackStream(Stream sourceStream, ICurrentDateTimeSource currentDateTimeSource, DateTime constructionTimestamp)
        {
            if (sourceStream == null)
                throw new ArgumentNullException("sourceStream");

            if (currentDateTimeSource == null)
                throw new ArgumentNullException("currentDateTimeSource");

            _sourceStream = sourceStream;

            _currentDateTimeSource = currentDateTimeSource;

            _constructionTimestamp = constructionTimestamp;

            CurrentPlaybackState = new ReadingTimeOffsetPlaybackState();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
                throw new IndexOutOfRangeException(ExceptionMessages.OffsetPlusCountGreaterThanBufferSizeMessage);

            lock (ReadSyncObject)
            {

                int read = 0;

                IPlaybackState lastState;

                do
                {
                    lastState = CurrentPlaybackState;

                    read += CurrentPlaybackState.Read(SourceStream, CurrentDateTimeSource.GetCurrentDateTime().Subtract(ConstructionTimestamp), buffer, offset + read, count - read);

                    CurrentPlaybackState = CurrentPlaybackState.StateTransition();
                }
                while (lastState != CurrentPlaybackState);

                return read;

            }

        }

    }

}
