using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal class CompletedWriteStreamBuffer : ICompletedWriteStreamHandler, ICompletedWriteStreamSource, IWriteStreamLengthValidator,
        IDisposable
    {

        private class CompletedWriteStreamDefinition
        {
            public Stream Stream { get; set; }
            public long Length { get; set; }
        }

        private readonly Queue<CompletedWriteStreamDefinition> _completedWriteStreamQueue = new Queue<CompletedWriteStreamDefinition>();

        private long TotalCompletedBytes { get; set; }

        private Queue<CompletedWriteStreamDefinition> CompletedWriteStreamQueue { get { return _completedWriteStreamQueue; } }

        private readonly object _completedWriteStreamQueueSyncObject = new object();

        private object CompletedWriteStreamQueueSyncObject { get { return _completedWriteStreamQueueSyncObject; } }

        private readonly IReadStreamQueueTarget _readStreamQueueTarget;

        public IReadStreamQueueTarget ReadStreamQueueTarget { get { return _readStreamQueueTarget; } }

        private readonly IWriteStreamToReadStreamConverter _writeStreamToReadStreamConverter;

        public IWriteStreamToReadStreamConverter WriteStreamToReadStreamConverter { get { return _writeStreamToReadStreamConverter; } }

        private readonly IStreamDataSourceDisposerFactory _readStreamDataSourceDisposerFactory;

        public IStreamDataSourceDisposerFactory ReadStreamDataSourceDisposerFactory { get { return _readStreamDataSourceDisposerFactory; } }

        private readonly IDataSourceDisposerRegister _dataSourceDisposerRegister;

        public IDataSourceDisposerRegister DataSourceDisposerRegister { get { return _dataSourceDisposerRegister; } }

        private readonly long? _maximumAllowedWriteStreamBytes;

        public long? MaximumAllowedWriteStreamBytes { get { return _maximumAllowedWriteStreamBytes; } }

        private bool Disposed { get; set; }

        public CompletedWriteStreamBuffer(IReadStreamQueueTarget readStreamQueueTarget,
            IWriteStreamToReadStreamConverter writeStreamToReadStreamConverter,
            IStreamDataSourceDisposerFactory readStreamDataSourceDisposerFactory,
            IDataSourceDisposerRegister dataSourceDisposerRegister,
            long? maximumAllowedWriteStreamBytes)
        {
            _readStreamQueueTarget = readStreamQueueTarget;
            _writeStreamToReadStreamConverter = writeStreamToReadStreamConverter;
            _maximumAllowedWriteStreamBytes = maximumAllowedWriteStreamBytes;
            _readStreamDataSourceDisposerFactory = readStreamDataSourceDisposerFactory;
            _dataSourceDisposerRegister = dataSourceDisposerRegister;
        }

        public void HandleCompletedWriteStream(Stream writeStream, long bytesWritten)
        {
            lock (CompletedWriteStreamQueueSyncObject)
            {
                CompletedWriteStreamQueue.Enqueue(new CompletedWriteStreamDefinition() { Stream = writeStream, Length = bytesWritten });

                TotalCompletedBytes += bytesWritten;
            }
        }

        public void FlushAllCompletedWriteStreamsToReadStreamQueue()
        {
            while (CompletedWriteStreamQueue.Count > 0)
            {
                lock (CompletedWriteStreamQueueSyncObject)
                {
                    FlushNextCompletedWriteStreamToReadStreamQueue();
                }
            }
        }
        
        public long RequestAvailableLength(long idealLength)
        {
            long output = idealLength;

            lock (CompletedWriteStreamQueueSyncObject)
            {

                if (MaximumAllowedWriteStreamBytes.HasValue
                    && MaximumAllowedWriteStreamBytes.Value - TotalCompletedBytes < idealLength)
                {

                    while (CompletedWriteStreamQueue.Count > 0 && MaximumAllowedWriteStreamBytes.HasValue
                        && MaximumAllowedWriteStreamBytes.Value - TotalCompletedBytes < idealLength)
                    {
                        FlushNextCompletedWriteStreamToReadStreamQueue();
                    }

                }

                output = idealLength;

                if (MaximumAllowedWriteStreamBytes.HasValue && output > MaximumAllowedWriteStreamBytes.Value - TotalCompletedBytes)
                {
                    output = MaximumAllowedWriteStreamBytes.Value - TotalCompletedBytes;
                }

            }

            return output;

        }

        private void FlushNextCompletedWriteStreamToReadStreamQueue()
        {
            if (CompletedWriteStreamQueue.Count > 0)
            {
                var nextStreamDefinition = CompletedWriteStreamQueue.Dequeue();

                var readStream = WriteStreamToReadStreamConverter.ConvertWriteStreamToReadStream(nextStreamDefinition.Stream);

                DataSourceDisposerRegister.Swap(nextStreamDefinition.Stream, readStream, ReadStreamDataSourceDisposerFactory.CreateStreamDataSourceDisposer(readStream));

                ReadStreamQueueTarget.EnqueueStream(new ReadStreamDefinition(readStream, nextStreamDefinition.Length));

                TotalCompletedBytes -= nextStreamDefinition.Length;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (CompletedWriteStreamQueueSyncObject)
            {

                if (!Disposed)
                {
                    if (disposing)
                    {
                        while (CompletedWriteStreamQueue.Count() > 0)
                        {
                            var streamDefinition = CompletedWriteStreamQueue.Dequeue();

                            streamDefinition.Stream.Close();

                            DataSourceDisposerRegister.Release(streamDefinition.Stream);
                        }
                    }

                    Disposed = true;
                }

            }
        }

    }

}
