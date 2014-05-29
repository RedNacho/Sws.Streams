using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common;

namespace Sws.Streams.Core.Rolling.Internal
{
    internal class RollingMemory : IRollingMemory
    {

        private readonly RollingMemoryReadStream _readStream;

        private readonly RollingMemoryWriteStream _writeStream;

        private readonly CompletedWriteStreamBuffer _completedWriteStreamBuffer;

        private bool Disposed { get; set; }

        private readonly object _disposalSyncObject = new object();

        private object DisposalSyncObject { get { return _disposalSyncObject; } }

        public RollingMemory(
            IWriteStreamFactory writeStreamFactory,
            IWriteStreamToReadStreamConverter writeStreamToReadStreamConverter,
            IStreamDataSourceDisposerFactory writeStreamDataSourceDisposerFactory,
            IStreamDataSourceDisposerFactory readStreamDataSourceDisposerFactory,
            IRollingMemoryStateMonitor rollingMemoryStateMonitor,
            long? maximumAllowedWriteStreamBytes, TimeSpan? maximumAllowedUnavailableAge,
            long? maximumAllowedUnavailableBytes, ICurrentDateTimeSource currentDateTimeSource)
        {

            var readStreamQueue = new ReadStreamQueue();

            var dataSourceDisposerRegister = new DataSourceDisposerRegister();

            var completedWriteStreamBuffer = new CompletedWriteStreamBuffer(
                readStreamQueue,
                writeStreamToReadStreamConverter,
                readStreamDataSourceDisposerFactory,
                dataSourceDisposerRegister,
                maximumAllowedWriteStreamBytes);

            var rollingMemoryWriteStream = new RollingMemoryWriteStream(
                writeStreamFactory,
                completedWriteStreamBuffer,
                completedWriteStreamBuffer,
                currentDateTimeSource,
                writeStreamDataSourceDisposerFactory,
                dataSourceDisposerRegister,
                rollingMemoryStateMonitor);

            var readStreamQueueRefresher = new ReadStreamQueueRefresher(
                rollingMemoryWriteStream,
                completedWriteStreamBuffer,
                maximumAllowedUnavailableAge,
                maximumAllowedUnavailableBytes);

            var completedReadStreamHandler = new CompletedReadStreamHandler(
                dataSourceDisposerRegister);

            var rollingMemoryReadStream = new RollingMemoryReadStream(
                readStreamQueue,
                readStreamQueueRefresher,
                completedReadStreamHandler,
                rollingMemoryStateMonitor);

            _readStream = rollingMemoryReadStream;

            _writeStream = rollingMemoryWriteStream;

            _completedWriteStreamBuffer = completedWriteStreamBuffer;
        }

        public Stream WriteStream
        {
            get { return _writeStream; }
        }

        public Stream ReadStream
        {
            get { return _readStream; }
        }

        private CompletedWriteStreamBuffer CompletedWriteStreamBuffer
        {
            get { return _completedWriteStreamBuffer; }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (DisposalSyncObject)
            {
                if (!Disposed)
                {
                    if (disposing)
                    {
                        WriteStream.Dispose();

                        CompletedWriteStreamBuffer.Dispose();

                        ReadStream.Dispose();
                    }
                }

                Disposed = true;
            }
        }

    }
}
