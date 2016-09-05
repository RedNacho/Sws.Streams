using System;
using System.IO;
using Sws.Streams.Core.Common.AbstractStreamImplementations;

namespace Sws.Streams.Core.Rolling.Internal
{
    internal class BlockForCapacityWriteStream : NonSeekableWriteOnlyStream
    {
        private readonly Stream _target;
        private readonly Func<long> _capacityFetcher;
        private readonly Action _waitForCapacityChange;
        private readonly Action _acquireCapacityChangeLock;
        private readonly Action _releaseCapacityChangeLock;

        public BlockForCapacityWriteStream(Stream target, Func<long> capacityFetcher, Action waitForCapacityChange, Action acquireCapacityChangeLock, Action releaseCapacityChangeLock)
        {
            _target = target;
            _capacityFetcher = capacityFetcher;
            _waitForCapacityChange = waitForCapacityChange;
            _acquireCapacityChangeLock = acquireCapacityChangeLock;
            _releaseCapacityChangeLock = releaseCapacityChangeLock;
        }

        public override void Flush()
        {
            _target.Flush();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _acquireCapacityChangeLock();

            try
            {
                while (count > 0)
                {
                    var capacity = _capacityFetcher();

                    var toWrite = capacity > count ? count : (int) capacity;

                    if (toWrite > 0)
                    {
                        _target.Write(buffer, offset, toWrite);
                        offset += toWrite;
                        count -= toWrite;
                    }
                    else if (count > 0)
                    {
                        _waitForCapacityChange();
                    }
                }
            }
            finally
            {
                _releaseCapacityChangeLock();
            }
        }

        public override bool CanWrite
        {
            get { return _target.CanWrite; }
        }
    }
}
