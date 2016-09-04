using System;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{
    internal class BlockingWriteRollingMemory : IRollingMemory
    {
        private readonly IRollingMemory _target;
        private readonly Stream _writeStream;

        public BlockingWriteRollingMemory(Func<IRollingMemoryStateMonitor, IRollingMemory> targetFactory,
            IRollingMemoryStateMonitor rollingMemoryStateMonitor,
            long blockWritesAboveLagBytes)
        {
            var lagCountingRollingMemoryStateMonitor = new LagCountingRollingMemoryStateMonitor();
            rollingMemoryStateMonitor = new CompositeRollingMemoryStateMonitor(rollingMemoryStateMonitor, lagCountingRollingMemoryStateMonitor);
            _target = targetFactory(rollingMemoryStateMonitor);
            _writeStream = new BlockForCapacityWriteStream(
                _target.WriteStream,
                () => blockWritesAboveLagBytes - lagCountingRollingMemoryStateMonitor.Lag,
                lagCountingRollingMemoryStateMonitor.WaitForLagChange,
                lagCountingRollingMemoryStateMonitor.AcquireLagChangeLock,
                lagCountingRollingMemoryStateMonitor.ReleaseLagChangeLock
            );
        }

        public void Dispose()
        {
            _target.Dispose();
        }

        public Stream WriteStream { get { return _writeStream; } }
        public Stream ReadStream { get { return _target.ReadStream; } }
    }
}
