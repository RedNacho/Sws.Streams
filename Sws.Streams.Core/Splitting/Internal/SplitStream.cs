using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common.AbstractStreamImplementations;
using Sws.Streams.Core.Common.Internal;

namespace Sws.Streams.Core.Splitting.Internal
{

    internal class SplitStream : NonSeekableWriteOnlyStream
    {

        private readonly IEnumerable<Stream> _targetStreams;

        public IEnumerable<Stream> TargetStreams { get { return _targetStreams; } }

        private readonly ISplitStreamExceptionHandler _exceptionHandler;

        public ISplitStreamExceptionHandler ExceptionHandler { get { return _exceptionHandler; } }

        private readonly object _syncObject = new object();

        private object SyncObject { get { return _syncObject; } }

        public SplitStream(IEnumerable<Stream> targetStreams, ISplitStreamExceptionHandler exceptionHandler)
        {
            if (targetStreams == null)
                targetStreams = Enumerable.Empty<Stream>();

            _targetStreams = targetStreams.ToArray();

            _exceptionHandler = exceptionHandler;
        }

        public override bool CanWrite
        {
            get
            {
                bool allCanWrite = true;

                allCanWrite = TryActOnAllStreams(stream => allCanWrite = allCanWrite && stream.CanWrite) && allCanWrite;

                return allCanWrite;
            }
        }

        public override void Flush()
        {
            TryActOnAllStreams(stream => stream.Flush());
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
                throw new IndexOutOfRangeException(ExceptionMessages.OffsetPlusCountGreaterThanBufferSizeMessage);

            TryActOnAllStreams(stream => stream.Write(buffer, offset, count));
        }

        private bool TryActOnAllStreams(Action<Stream> action)
        {
            bool output = true;

            lock (SyncObject)
            {
                foreach (var targetStream in TargetStreams)
                {
                    try
                    {
                        action(targetStream);
                    }
                    catch (Exception exception)
                    {
                        if (ExceptionHandler != null)
                        {
                            ExceptionHandler.HandleException(targetStream, exception);
                        }
                        else
                        {
                            throw;
                        }
                        output = false;
                    }
                }
            }

            return output;
        }

    }

}
