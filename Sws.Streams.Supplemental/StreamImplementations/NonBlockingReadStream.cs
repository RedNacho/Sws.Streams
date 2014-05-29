using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common.AbstractStreamImplementations;

namespace Sws.Streams.Supplemental.StreamImplementations
{

    public class NonBlockingReadStream : NonSeekableReadOnlyStream
    {

        private readonly Stream _sourceStream;

        public Stream SourceStream { get { return _sourceStream; } }

        private readonly Func<bool> _shouldTryReadChecker;

        public Func<bool> ShouldTryReadChecker { get { return _shouldTryReadChecker; } }

        private readonly object _readSyncObject = new object();

        private object ReadSyncObject { get { return _readSyncObject; } }

        public NonBlockingReadStream(Stream sourceStream, Func<bool> shouldTryReadChecker)
        {
            _sourceStream = sourceStream;
            _shouldTryReadChecker = shouldTryReadChecker;
        }

        public override bool CanRead
        {
            get
            {
                return SourceStream.CanRead;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;

            lock (ReadSyncObject)
            {

                if (ShouldTryReadChecker())
                {
                    read = SourceStream.Read(buffer, offset, count);
                }

            }

            return read;
        }

    }

}
