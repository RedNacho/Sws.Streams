using System;
using Sws.Streams.Core.Rolling.MemoryStreamBased;
using Sws.Streams.Core.Rolling.Internal;
using Sws.Streams.Core.Common;
using Sws.Streams.Core.Common.Internal;

namespace Sws.Streams.Core.Rolling
{
    public class RollingMemoryBuilder
    {

        private Lazy<IWriteStreamFactory> _writeStreamFactory = new Lazy<IWriteStreamFactory>(() => new MemoryStreamWriteStreamFactory(), true);

        public IWriteStreamFactory WriteStreamFactory { get { return _writeStreamFactory.Value; } }

        private Lazy<IWriteStreamToReadStreamConverter> _writeStreamToReadStreamConverter = new Lazy<IWriteStreamToReadStreamConverter>(() => new MemoryStreamWriteStreamToReadStreamConverter(), true);

        public IWriteStreamToReadStreamConverter WriteStreamToReadStreamConverter { get { return _writeStreamToReadStreamConverter.Value; } }

        private Lazy<IStreamDataSourceDisposerFactory> _writeStreamDataSourceDisposerFactory = new Lazy<IStreamDataSourceDisposerFactory>(() => new MemoryStreamStreamDataSourceDisposerFactory(), true);

        public IStreamDataSourceDisposerFactory WriteStreamDataSourceDisposerFactory { get { return _writeStreamDataSourceDisposerFactory.Value; } }

        private Lazy<IStreamDataSourceDisposerFactory> _readStreamDataSourceDisposerFactory = new Lazy<IStreamDataSourceDisposerFactory>(() => new MemoryStreamStreamDataSourceDisposerFactory(), true);

        public IStreamDataSourceDisposerFactory ReadStreamDataSourceDisposerFactory { get { return _readStreamDataSourceDisposerFactory.Value; } }

        private Lazy<ICurrentDateTimeSource> _currentDateTimeSource = new Lazy<ICurrentDateTimeSource>(() => new CurrentDateTimeSource(), true);

        public ICurrentDateTimeSource CurrentDateTimeSource { get { return _currentDateTimeSource.Value; } }

        private long? _maximumAllowedWriteStreamBytes = null;

        public long? MaximumAllowedWriteStreamBytes { get { return _maximumAllowedWriteStreamBytes; } }

        private TimeSpan? _maximumAllowedUnavailableAge = null;

        public TimeSpan? MaximumAllowedUnavailableAge { get { return _maximumAllowedUnavailableAge; } }

        private long? _maximumAllowedUnavailableBytes = 0;

        public long? MaximumAllowedUnavailableBytes { get { return _maximumAllowedUnavailableBytes; } }

        private long? _blockWritesAboveLagBytes = null;

        public long? BlockWritesAboveLagBytes { get { return _blockWritesAboveLagBytes; } }

        private IRollingMemoryStateMonitor _rollingMemoryStateMonitor = null;

        public IRollingMemoryStateMonitor RollingMemoryStateMonitor { get { return _rollingMemoryStateMonitor; } }

        /// <summary>
        /// Overrides the default Stream lifecycle implementation.  See interface definitions for more details.
        /// </summary>
        /// <param name="writeStreamFactory">Creates sequentially writable Streams.  These Streams won't last long, so the primary
        /// concern will be that they should be fast.  This will be invoked on the thread(s) writing to the rolling memory.</param>
        /// <param name="writeStreamToReadStreamConverter">Converts Streams created by writeStreamFactory to sequentially readable
        /// Streams.  Ordinarily data will sit in these Streams between being written and being read, so an underlying storage
        /// medium which can buffer the expected amount of data is preferable.  This will ordinarily be invoked on the thread(s)
        /// reading from the rolling memory, although it may be invoked during writing if clearing space becomes an immediate
        /// requirement.</param>
        /// <param name="writeStreamDataSourceDisposerFactory">Creates IDataSourceDisposers for Streams created by
        /// writeStreamFactory.</param>
        /// <param name="readStreamDataSourceDisposerFactory">Creates IDataSourceDisposers for Streams returned by
        /// IWriteStreamToReadStreamConverter.</param>
        /// <returns></returns>

        public RollingMemoryBuilder SetupStreamLifecycleImplementation(
            IWriteStreamFactory writeStreamFactory, IWriteStreamToReadStreamConverter writeStreamToReadStreamConverter,
            IStreamDataSourceDisposerFactory writeStreamDataSourceDisposerFactory, IStreamDataSourceDisposerFactory readStreamDataSourceDisposerFactory)
        {
            if (writeStreamFactory == null)
                throw new ArgumentNullException("writeStreamFactory");

            if (writeStreamToReadStreamConverter == null)
                throw new ArgumentNullException("writeStreamToReadStreamConverter");

            if (writeStreamDataSourceDisposerFactory == null)
                throw new ArgumentNullException("writeStreamDataSourceDisposerFactory");

            if (readStreamDataSourceDisposerFactory == null)
                throw new ArgumentNullException("readStreamDataSourceDisposerFactory");

            _writeStreamFactory = new Lazy<IWriteStreamFactory>(() => writeStreamFactory, true);

            _writeStreamToReadStreamConverter = new Lazy<IWriteStreamToReadStreamConverter>(() => writeStreamToReadStreamConverter, true);

            _writeStreamDataSourceDisposerFactory = new Lazy<IStreamDataSourceDisposerFactory>(() => writeStreamDataSourceDisposerFactory, true);

            _readStreamDataSourceDisposerFactory = new Lazy<IStreamDataSourceDisposerFactory>(() => readStreamDataSourceDisposerFactory, true);

            return this;
        }

        /// <summary>
        /// Sets the maximum allowed number of bytes on WriteStream; if null, unlimited.  This is the maximum amount of data
        /// that can have been written on all Streams which have been created by WriteStreamFactory but not yet converted by
        /// WriteStreamToReadStreamConverter.  If a Write operation will cause this value to be breached,
        /// WriteStreamToReadStreamConverter will be invoked in order to bring the amount of data down.  Ordinarily, Read operations
        /// are expected to trigger conversion; this value is a safeguard which will trigger conversion in the case that there are no
        /// Read operations.  The default is null (no limit).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public RollingMemoryBuilder SetMaximumAllowedWriteStreamBytes(long? value)
        {
            if (value <= 0)
                throw new ArgumentException(string.Format(ExceptionMessages.ValueMustBeGreaterThanZeroFormat, "value"), "value");

            _maximumAllowedWriteStreamBytes = value;

            return this;
        }

        /// <summary>
        /// Sets the maximum allowed age of data that has been written before it must be available on the ReadStream.
        /// In other words, this guarantees that all data which is older than this is available for reading.  A
        /// null value indicates no maximum, this is the default.  Either this value or MaximumAllowedUnavailableBytes
        /// should be non-null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public RollingMemoryBuilder SetMaximumAllowedUnavailableAge(TimeSpan? value)
        {
            _maximumAllowedUnavailableAge = value;

            return this;
        }

        /// <summary>
        /// Blocks writes when the write position is beyond this amount ahead of the read
        /// position. This can be used to create backpressure, but it should be used very carefully!
        /// Reads and Writes must always take place on separate threads to avoid deadlocks.
        /// If this value is set, MaximumAllowedUnavailableAge must also be set, or
        /// MaximumAllowedUnavailableBytes must be set to a value less than this one.
        /// This is because otherwise it is possible for the amount of data unavailable
        /// for reading to become permanently greater than the maximum allowed lag, creating
        /// another deadlock situation.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public RollingMemoryBuilder SetBlockWritesAboveLagBytes(long? value)
        {
            _blockWritesAboveLagBytes = value;

            return this;
        }

        /// <summary>
        /// Sets the maximum amount of data that can have been written to the WriteStream without being available on the ReadStream.
        /// In other words, this guarantees that no more than this number of bytes will be unavailable for reading.  A null
        /// value indicates no maximum.  The default is zero, which means that all data is immediately available for reading;
        /// although this is not recommended as it may adversely affect performance.  (You can use WriteStream.Flush() to force
        /// all data to be available prior to a particular Read.)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public RollingMemoryBuilder SetMaximumAllowedUnavailableBytes(long? value)
        {
            _maximumAllowedUnavailableBytes = value;

            return this;
        }

        /// <summary>
        /// Allows you to register a monitor which will be continually updated whenever the write position or read position changes.
        /// You can use this to monitor the total amount of data written and read.  At any given time, the difference between the
        /// total written and the total read should be a rough indicator of the amount of data which is currently stored in
        /// rolling memory.  Each method is individually thread-safe, and both should be lightweight.  The default is null,
        /// which means no monitoring.
        /// </summary>
        /// <param name="rollingMemoryStateMonitor"></param>
        /// <returns></returns>

        public RollingMemoryBuilder SetRollingMemoryStateMonitor(IRollingMemoryStateMonitor rollingMemoryStateMonitor)
        {
            _rollingMemoryStateMonitor = rollingMemoryStateMonitor;

            return this;
        }

        /// <summary>
        /// Sets all of the usual Stream lifecycle thresholds in one call.  See Set... methods for each
        /// for more information.
        /// </summary>
        /// <param name="maximumAllowedWriteStreamBytes"></param>
        /// <param name="maximumAllowedUnavailableAge"></param>
        /// <param name="maximumAllowedUnavailableBytes"></param>
        /// <returns></returns>

        public RollingMemoryBuilder SetStreamLifecycleThresholds(long? maximumAllowedWriteStreamBytes,
            TimeSpan? maximumAllowedUnavailableAge,
            long? maximumAllowedUnavailableBytes)
        {
            return this.SetMaximumAllowedWriteStreamBytes(maximumAllowedWriteStreamBytes)
                .SetMaximumAllowedUnavailableAge(maximumAllowedUnavailableAge)
                .SetMaximumAllowedUnavailableBytes(maximumAllowedUnavailableBytes);
        }

        /// <summary>
        /// Allows the default CurrentDateTimeSource to be overridden.  This simply provides the current
        /// DateTime from the system; the ability to override it is provided to facilitate unit testing.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public RollingMemoryBuilder SetCurrentDateTimeSource(ICurrentDateTimeSource value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _currentDateTimeSource = new Lazy<ICurrentDateTimeSource>(() => value, true);

            return this;
        }

        /// <summary>
        /// Builds an IRollingMemory based on the current configuration.
        /// </summary>
        /// <returns></returns>

        public IRollingMemory Build()
        {
            Func<IRollingMemoryStateMonitor, IRollingMemory> rollingMemoryBuildFunction = BuildCoreRollingMemory;

            if (BlockWritesAboveLagBytes.HasValue)
            {
                if ((!MaximumAllowedUnavailableAge.HasValue) &&
                    MaximumAllowedUnavailableBytes.GetValueOrDefault(BlockWritesAboveLagBytes.Value + 1) >= BlockWritesAboveLagBytes.Value)
                {
                    throw new ArgumentException("If BlockWritesAboveLagBytes is specified, either MaximumAllowedUnavailableAge must be set, or MaximumAllowedUnavailableBytes must be lower than BlockWritesAboveLagBytes. This is to prevent the rolling memory entering a state where the amount of data which is unavailable for reading is greater than or equal to the allowed lag.");
                }

                var innerRollingMemoryBuildFunction = rollingMemoryBuildFunction;

                rollingMemoryBuildFunction = rollingMemoryStateMonitor => new BlockingWriteRollingMemory(innerRollingMemoryBuildFunction, rollingMemoryStateMonitor, BlockWritesAboveLagBytes.Value);
            }

            return rollingMemoryBuildFunction(RollingMemoryStateMonitor);
        }

        private IRollingMemory BuildCoreRollingMemory(IRollingMemoryStateMonitor rollingMemoryStateMonitor)
        {
            return new RollingMemory(
                WriteStreamFactory,
                WriteStreamToReadStreamConverter,
                WriteStreamDataSourceDisposerFactory,
                ReadStreamDataSourceDisposerFactory,
                rollingMemoryStateMonitor,
                MaximumAllowedWriteStreamBytes,
                MaximumAllowedUnavailableAge,
                MaximumAllowedUnavailableBytes,
                CurrentDateTimeSource
            );
        }
    }
}
