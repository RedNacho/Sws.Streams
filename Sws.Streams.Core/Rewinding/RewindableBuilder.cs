using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Rewinding.Internal;
using Sws.Streams.Core.Rewinding.MemoryStreamBased;

namespace Sws.Streams.Core.Rewinding
{
    public class RewindableBuilder
    {

        private Lazy<ICacheAccessor> _cacheAccessor = new Lazy<ICacheAccessor>(() => new MemoryStreamCacheAccessor());

        public ICacheAccessor CacheAccessor { get { return _cacheAccessor.Value; } }

        private Stream _sourceStream;

        public Stream SourceStream { get { return _sourceStream; } }

        private long? _autoTruncateAtBytes = null;

        public long? AutoTruncateAtBytes { get { return _autoTruncateAtBytes; } }

        private long? _autoTruncationTailLength = null;

        public long? AutoTruncationTailLength { get { return _autoTruncationTailLength; } }

        /// <summary>
        /// Creates a RewindableStreamBuilder which will wrap a given sourceStream.  This
        /// may be modified later, but it cannot be null.
        /// </summary>
        /// <param name="sourceStream"></param>

        public RewindableBuilder(Stream sourceStream)
        {
            if (sourceStream == null)
                throw new ArgumentNullException("sourceStream");

            _sourceStream = sourceStream;
        }

        /// <summary>
        /// Overrides the CacheAccessor.  (By default, this is MemoryStream-based).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public RewindableBuilder SetCacheAccessor(ICacheAccessor value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _cacheAccessor = new Lazy<ICacheAccessor>(() => value);

            return this;
        }

        /// <summary>
        /// Specifies the value beyond which the cache will not be allowed to grow.  When this value is reached,
        /// the cache will be truncated.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public RewindableBuilder SetAutoTruncateAtBytes(long? value)
        {
            _autoTruncateAtBytes = value;

            return this;
        }

        /// <summary>
        /// In the event that truncating the cache is an expensive operation, this value can be used to ensure that
        /// auto-truncate operations take place less frequently.  e.g. If you specify AutoTruncateAtBytes to 10 bytes,
        /// and AutoTruncationTailLength to 5 bytes, when the cache hits 10 bytes, it will be truncated to 5 bytes,
        /// so that it has a little room to grow before auto-truncation kicks in again.  Setting this value to 0
        /// will cause the cache to be completely cleared when AutoTruncateAtBytes is reached.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public RewindableBuilder SetAutoTruncationTailLength(long? value)
        {
            _autoTruncationTailLength = value;

            return this;
        }

        /// <summary>
        /// Sets the Stream to be wrapped by the RewindableStream.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public RewindableBuilder SetSourceStream(Stream value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _sourceStream = value;

            return this;
        }

        /// <summary>
        /// Builds the RewindableStream based on the current configuration.
        /// </summary>
        /// <returns></returns>

        public IRewindable Build()
        {
            return new Rewindable(
                new RewindableStream(SourceStream, CacheAccessor, AutoTruncateAtBytes, AutoTruncationTailLength)
            );
        }

    }
}
