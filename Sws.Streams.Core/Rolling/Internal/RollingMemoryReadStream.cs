using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common.AbstractStreamImplementations;
using Sws.Streams.Core.Common.Internal;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal class RollingMemoryReadStream : NonSeekableReadOnlyStream
    {

        private readonly IReadStreamQueueSource _sourceQueue;

        public IReadStreamQueueSource SourceQueue { get { return _sourceQueue; } }

        private readonly IReadStreamQueueRefresher _readStreamQueueRefresher;

        public IReadStreamQueueRefresher ReadStreamQueueRefresher { get { return _readStreamQueueRefresher; } }

        private readonly ICompletedReadStreamHandler _completedReadStreamHandler;

        public ICompletedReadStreamHandler CompletedReadStreamHandler { get { return _completedReadStreamHandler; } }

        private readonly IRollingMemoryStateMonitor _rollingMemoryStateMonitor;

        public IRollingMemoryStateMonitor RollingMemoryStateMonitor { get { return _rollingMemoryStateMonitor; } }

        private readonly object _CurrentSourceSyncObject = new object();

        private object CurrentSourceSyncObject { get { return _CurrentSourceSyncObject; } }

        public ReadStreamDefinition CurrentSource {
            get;
            private set;
        }

        private long CurrentSourcePosition { get; set; }

        public RollingMemoryReadStream(IReadStreamQueueSource sourceQueue,
            IReadStreamQueueRefresher readStreamQueueRefresher,
            ICompletedReadStreamHandler completedReadStreamHandler,
            IRollingMemoryStateMonitor rollingMemoryStateMonitor)
        {
            _sourceQueue = sourceQueue;
            _readStreamQueueRefresher = readStreamQueueRefresher;
            _completedReadStreamHandler = completedReadStreamHandler;
            _rollingMemoryStateMonitor = rollingMemoryStateMonitor;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
                throw new IndexOutOfRangeException(ExceptionMessages.OffsetPlusCountGreaterThanBufferSizeMessage);

            int read = 0;

            lock (CurrentSourceSyncObject)
            {

                SwitchSourceIfNecessary(true);

                bool newSource = true;

                while (read < count && CurrentSource != null && newSource)
                {

                    if (CurrentSource != null)
                    {
                        int expectedRead = (int)Math.Min(count - read, CurrentSource.Length - CurrentSourcePosition);

                        int readIncrement = 0;

                        if (expectedRead > 0)
                            readIncrement = CurrentSource.Stream.Read(buffer, offset + read, expectedRead);

                        read += readIncrement;

                        CurrentSourcePosition += readIncrement;

                        newSource = SwitchSourceIfNecessary(false);

                    }

                }

                if (RollingMemoryStateMonitor != null)
                {
                    RollingMemoryStateMonitor.ReadPositionAdvanced(read);
                }

            }

            return read;
        }

        private bool SwitchSourceIfNecessary(bool alwaysRefreshSources)
        {
            bool output = false;

            bool switchSource = (CurrentSource == null || CurrentSourcePosition >= CurrentSource.Length);

            if (alwaysRefreshSources || switchSource)
            {

                RefreshReadStreamQueue();

            }

            if (switchSource)
            {
                SwitchSource();

                output = true;
            }

            return output;
        }

        private void RefreshReadStreamQueue()
        {

            ReadStreamQueueRefresher.RefreshReadStreamQueue();

        }

        private void SwitchSource()
        {
            if (CurrentSource != null)
            {
                CompletedReadStreamHandler.HandleCompletedReadStream(CurrentSource.Stream);
            }

            CurrentSource = SourceQueue.DequeueStream();

            CurrentSourcePosition = 0;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (CurrentSourceSyncObject)
                {

                    if (CurrentSource != null)
                    {
                        CompletedReadStreamHandler.HandleCompletedReadStream(CurrentSource.Stream);
                    }

                    ReadStreamDefinition readStreamDefinition;

                    while ((readStreamDefinition = SourceQueue.DequeueStream()) != null)
                    {
                        CompletedReadStreamHandler.HandleCompletedReadStream(readStreamDefinition.Stream);
                    }

                }
            }

            base.Dispose(disposing);
        }

    }

}
