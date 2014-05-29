using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common.AbstractStreamImplementations;
using Sws.Streams.Core.Common.Internal;

namespace Sws.Streams.Core.Rewinding.Internal
{
    internal class RewindableStream : NonSeekableReadOnlyStream
    {
        
        private readonly Stream _sourceStream;

        public Stream SourceStream { get { return _sourceStream; } }

        private readonly ICacheAccessor _cacheAccessor;

        public ICacheAccessor CacheAccessor { get { return _cacheAccessor; } }

        private long DistanceFromCacheEnd { get; set; }

        private readonly object _cacheSyncObject = new object();

        private object CacheSyncObject { get { return _cacheSyncObject; } }

        private readonly long? _autoTruncateAtBytes;

        public long? AutoTruncateAtBytes { get { return _autoTruncateAtBytes; } }

        private readonly long? _autoTruncationTailLength;

        public long? AutoTruncationTailLength { get { return _autoTruncationTailLength; } }

        private long? AutoTruncateCurrentLength { get; set; }

        public RewindableStream(Stream sourceStream, ICacheAccessor cacheAccessor,
            long? autoTruncateAtBytes, long? autoTruncationTailLength)
        {
            _sourceStream = sourceStream;

            _cacheAccessor = cacheAccessor;

            _autoTruncateAtBytes = autoTruncateAtBytes;

            _autoTruncationTailLength = autoTruncationTailLength;
        }

        public override bool CanRead
        {
            get { return SourceStream.CanRead; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
                throw new IndexOutOfRangeException(ExceptionMessages.OffsetPlusCountGreaterThanBufferSizeMessage);

            lock (CacheSyncObject)
            {

                int readFromCache = (int)Math.Min(DistanceFromCacheEnd, count);

                int readFromMemory = count - readFromCache;

                int actualReadFromCache = 0;

                int actualRead;

                if (readFromCache > 0)
                {
                    CacheAccessor.ActOnCacheReadStream(cacheReadStream =>
                            actualReadFromCache = cacheReadStream.Read(buffer, offset, readFromCache),
                        DistanceFromCacheEnd);

                    DistanceFromCacheEnd -= actualReadFromCache;
                }

                int actualReadFromMemory = 0;

                if (actualReadFromCache == readFromCache && readFromMemory > 0)
                {
                    actualReadFromMemory = SourceStream.Read(buffer, offset + readFromCache, readFromMemory);
                }

                WriteToCache(buffer, offset + readFromCache, actualReadFromMemory);

                actualRead = actualReadFromCache + actualReadFromMemory;

                if (PositionIncremented != null)
                {
                    PositionIncremented(this, new PositionChangedEventArgs(actualRead));
                }

                return actualRead;

            }
        }

        private void WriteToCache(byte[] buffer, int offset, int count)
        {
            if (AutoTruncateAtBytes.HasValue && AutoTruncateAtBytes.Value <= 0)
                return;

            var writtenToCache = 0;

            while (writtenToCache < count)
            {

                var writeToCache = count - writtenToCache;

                if (AutoTruncateAtBytes.HasValue)
                {

                    if (AutoTruncateAtBytes.Value >= AutoTruncateCurrentLength.GetValueOrDefault())
                    {
                        var truncationTailLength = AutoTruncateAtBytes.Value;

                        truncationTailLength = Math.Max(truncationTailLength - writeToCache, 0);

                        truncationTailLength = Math.Min(truncationTailLength, AutoTruncationTailLength.GetValueOrDefault(AutoTruncateAtBytes.Value));

                        CacheAccessor.TruncateCache(truncationTailLength);
                            
                        AutoTruncateCurrentLength = truncationTailLength;
                    }

                    writeToCache = (int)Math.Min(AutoTruncateAtBytes.Value - AutoTruncateCurrentLength.GetValueOrDefault(), writeToCache);

                }

                if (writeToCache > 0)
                {

                    CacheAccessor.ActOnCacheWriteStream(cacheWriteStream =>
                        cacheWriteStream.Write(buffer, offset + writtenToCache, writeToCache));

                }

                writtenToCache += writeToCache;

                if (AutoTruncateAtBytes.HasValue)
                {
                    AutoTruncateCurrentLength = AutoTruncateCurrentLength.GetValueOrDefault() + writeToCache;
                }

            }

        }

        public virtual void Rewind(long step)
        {
            lock (CacheSyncObject)
            {
                DistanceFromCacheEnd += step;

                if (PositionDecremented != null)
                {
                    PositionDecremented(this, new PositionChangedEventArgs(step));
                }
            }
        }

        public virtual void Truncate(long maximumRewindStep)
        {
            lock (CacheSyncObject)
            {
                CacheAccessor.TruncateCache(maximumRewindStep + DistanceFromCacheEnd);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                lock (CacheSyncObject)
                {
                    CacheAccessor.DestroyCache();
                }
            }

        }

        public event PositionChangedEventHandler PositionIncremented;

        public event PositionChangedEventHandler PositionDecremented;

    }
}
