using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal class ReadStreamQueueRefresher : IReadStreamQueueRefresher
    {

        private readonly IActiveWriteStreamSource _activeWriteStreamSource;

        public IActiveWriteStreamSource ActiveWriteStreamSource { get { return _activeWriteStreamSource; } }

        private readonly ICompletedWriteStreamSource _completedWriteStreamSource;

        public ICompletedWriteStreamSource CompletedWriteStreamSource { get { return _completedWriteStreamSource; } }

        private readonly TimeSpan? _maximumAllowedUnavailableAge;

        public TimeSpan? MaximumAllowedUnavailableAge { get { return _maximumAllowedUnavailableAge; } }

        private readonly long? _maximumAllowedUnavailableBytes;

        public long? MaximumAllowedUnavailableBytes { get { return _maximumAllowedUnavailableBytes; } }

        private readonly object _refreshReadStreamQueueSyncObject = new object();

        private object RefreshReadStreamQueueSyncObject { get { return _refreshReadStreamQueueSyncObject; } }

        public ReadStreamQueueRefresher(
            IActiveWriteStreamSource activeWriteStreamSource,
            ICompletedWriteStreamSource completedWriteStreamSource,
            TimeSpan? maximumAllowedUnavailableAge,
            long? maximumAllowedUnavailableBytes)
        {
            _activeWriteStreamSource = activeWriteStreamSource;

            _completedWriteStreamSource = completedWriteStreamSource;

            _maximumAllowedUnavailableAge = maximumAllowedUnavailableAge;

            _maximumAllowedUnavailableBytes = maximumAllowedUnavailableBytes;
        }

        public void RefreshReadStreamQueue()
        {
            lock (RefreshReadStreamQueueSyncObject)
            {
                ActiveWriteStreamSource.EnsureTargetHasMaximumLengthAndAge(_maximumAllowedUnavailableBytes, _maximumAllowedUnavailableAge);

                CompletedWriteStreamSource.FlushAllCompletedWriteStreamsToReadStreamQueue();
            }
        }

    }

}
