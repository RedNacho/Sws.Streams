using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common;
using Sws.Streams.Core.Common.AbstractStreamImplementations;
using Sws.Streams.Core.Common.Internal;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal class RollingMemoryWriteStream : NonSeekableWriteOnlyStream, IActiveWriteStreamSource
    {

        private readonly IWriteStreamFactory _targetFactory;

        public IWriteStreamFactory TargetFactory { get { return _targetFactory; } }

        private readonly ICompletedWriteStreamHandler _completedWriteStreamHandler;

        public ICompletedWriteStreamHandler CompletedWriteStreamHandler { get { return _completedWriteStreamHandler; } }

        private readonly IWriteStreamLengthValidator _activeWriteStreamLengthValidator;

        public IWriteStreamLengthValidator ActiveWriteStreamLengthValidator { get { return _activeWriteStreamLengthValidator; } }

        private readonly IStreamDataSourceDisposerFactory _writeStreamDataSourceDisposerFactory;

        public IStreamDataSourceDisposerFactory WriteStreamDataSourceDisposerFactory { get { return _writeStreamDataSourceDisposerFactory; } }

        private readonly IDataSourceDisposerRegister _dataSourceDisposerRegister;

        public IDataSourceDisposerRegister DataSourceDisposerRegister { get { return _dataSourceDisposerRegister; } }

        private readonly ICurrentDateTimeSource _currentDateTimeSource;

        public ICurrentDateTimeSource CurrentDateTimeSource { get { return _currentDateTimeSource; } }

        private readonly IRollingMemoryStateMonitor _rollingMemoryStateMonitor;

        public IRollingMemoryStateMonitor RollingMemoryStateMonitor { get { return _rollingMemoryStateMonitor; } }

        private readonly object _currentTargetSyncObject = new object();

        private object CurrentTargetSyncObject { get { return _currentTargetSyncObject; } }

        private long TargetBytesWritten { get; set; }

        private DateTime? TargetSwitched { get; set; }

        private readonly object _targetStateReadSyncObject = new object();

        private object TargetStateReadSyncObject { get { return _targetStateReadSyncObject; } }

        private bool _disposed = false;

        public bool Disposed { get { return _disposed; } }

        public Stream CurrentTarget
        {
            get;
            private set;
        }

        public RollingMemoryWriteStream(IWriteStreamFactory targetFactory,
            ICompletedWriteStreamHandler completedWriteStreamHandler,
            IWriteStreamLengthValidator activeWriteStreamLengthValidator,
            ICurrentDateTimeSource currentDateTimeSource,
            IStreamDataSourceDisposerFactory writeStreamDataSourceDisposerFactory,
            IDataSourceDisposerRegister dataSourceDisposerRegister,
            IRollingMemoryStateMonitor rollingMemoryStateMonitor)
        {
            _targetFactory = targetFactory;

            _completedWriteStreamHandler = completedWriteStreamHandler;

            _activeWriteStreamLengthValidator = activeWriteStreamLengthValidator;

            _currentDateTimeSource = currentDateTimeSource;

            _writeStreamDataSourceDisposerFactory = writeStreamDataSourceDisposerFactory;

            _dataSourceDisposerRegister = dataSourceDisposerRegister;

            _rollingMemoryStateMonitor = rollingMemoryStateMonitor;
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            SwitchTargetIfDataWritten(false);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (Disposed)
                throw new ObjectDisposedException("RollingMemoryWriteStream");

            if (buffer.Length < offset + count)
                throw new IndexOutOfRangeException(ExceptionMessages.OffsetPlusCountGreaterThanBufferSizeMessage);

            lock (CurrentTargetSyncObject)
            {
                int written = 0;

                while (written < count)
                {

                    int availableToWrite = EnsureTargetAcceptsMaximumCount(count - written, true);

                    CurrentTarget.Write(buffer, offset + written, availableToWrite);

                    written += availableToWrite;

                    lock (TargetStateReadSyncObject)
                    {

                        TargetBytesWritten += availableToWrite;

                        if (!TargetSwitched.HasValue) TargetSwitched = CurrentDateTimeSource.GetCurrentDateTime();

                    }
                }

                if (RollingMemoryStateMonitor != null)
                {
                    RollingMemoryStateMonitor.WritePositionAdvanced(count);
                }

            }

        }

        public void EnsureTargetHasMaximumLengthAndAge(long? length, TimeSpan? age)
        {
            long targetLength;
            DateTime? targetSwitched;

            lock (TargetStateReadSyncObject)
            {
                targetLength = TargetBytesWritten;
                targetSwitched = TargetSwitched;
            }

            if (DoesTargetExceedMaximumLengthOrAge(targetLength, targetSwitched, length, age))
            {

                lock (CurrentTargetSyncObject)
                {
                    if (DoesTargetExceedMaximumLengthOrAge(TargetBytesWritten, TargetSwitched, length, age))
                    {
                        SwitchTarget(true);
                    }
                }

            }

        }

        private bool DoesTargetExceedMaximumLengthOrAge(long targetLength, DateTime? targetSwitched, long? length, TimeSpan? age)
        {
            return ((length.HasValue && targetLength > length.Value)
                     || (age.HasValue && ((targetSwitched.HasValue) && CurrentDateTimeSource.GetCurrentDateTime().Subtract(targetSwitched.Value) >= age)));
        }

        private int EnsureTargetAcceptsMaximumCount(int count, bool targetLocked)
        {
            int output;

            if (!targetLocked)
            {
                lock (CurrentTargetSyncObject)
                {
                    output = EnsureTargetAcceptsMaximumCount(count, true);
                }
            }
            else
            {
                var availableLength = ActiveWriteStreamLengthValidator.RequestAvailableLength(TargetBytesWritten + count);

                if (CurrentTarget == null || (TargetBytesWritten > 0 && TargetBytesWritten + count > availableLength))
                {
                    SwitchTarget(true);

                    availableLength = ActiveWriteStreamLengthValidator.RequestAvailableLength(TargetBytesWritten + count);
                }

                output = count;

                if (output > availableLength)
                {
                    output = (int)availableLength;
                }
            }

            return output;
        }

        private void SwitchTargetIfDataWritten(bool targetLocked)
        {
            SwitchTarget(targetLocked, true);
        }

        private void SwitchTarget(bool targetLocked, bool onlySwitchIfDataWritten = false)
        {
            if (!targetLocked)
            {
                lock (CurrentTargetSyncObject)
                {
                    SwitchTarget(true);
                }
            }
            else
            {
                if (onlySwitchIfDataWritten && TargetBytesWritten == 0)
                {
                    return;
                }

                if (CurrentTarget != null)
                {
                    CompletedWriteStreamHandler.HandleCompletedWriteStream(CurrentTarget, TargetBytesWritten);
                }

                if (Disposed)
                    throw new ObjectDisposedException("RollingMemoryWriteStream");

                CurrentTarget = TargetFactory.CreateWriteStream();

                DataSourceDisposerRegister.Register(CurrentTarget, WriteStreamDataSourceDisposerFactory.CreateStreamDataSourceDisposer(CurrentTarget));

                lock (TargetStateReadSyncObject)
                {

                    TargetBytesWritten = 0;

                    TargetSwitched = null;

                }

            }
        }

        protected override void Dispose(bool disposing)
        {

            lock (CurrentTargetSyncObject)
            {
                if (disposing)
                {

                    if ((!Disposed) && CurrentTarget != null)
                    {
                        CompletedWriteStreamHandler.HandleCompletedWriteStream(CurrentTarget, TargetBytesWritten);

                        CurrentTarget = null;

                        TargetBytesWritten = 0;

                        TargetSwitched = null;
                    }

                    _disposed = true;
                }
                
            }

            base.Dispose(disposing);
        }

    }

}
