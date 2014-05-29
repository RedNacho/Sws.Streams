using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sws.Streams.Core.Common;
using Sws.Streams.Core.Common.Internal;
using System.IO;

namespace Sws.Streams.Core.Forwarding.Internal
{
    internal class StreamForwarderRepeatingTask : IRepeatingTask
    {

        private readonly Stream _sourceStream;

        public Stream SourceStream { get { return _sourceStream; } }

        private readonly Stream _targetStream;

        public Stream TargetStream { get { return _targetStream; } }

        private int BufferWritePosition { get; set; }

        private readonly byte[] _buffer;

        public byte[] Buffer { get { return _buffer; } }

        private readonly IStreamAvailabilityChecker _sourceStreamAvailabilityChecker;

        public IStreamAvailabilityChecker SourceStreamAvailabilityChecker { get { return _sourceStreamAvailabilityChecker; } }

        private readonly IStreamCompletionChecker _sourceStreamCompletionChecker;

        public IStreamCompletionChecker SourceStreamCompletionChecker { get { return _sourceStreamCompletionChecker; } }

        public StreamForwarderRepeatingTask(Stream sourceStream, Stream targetStream, byte[] buffer, IStreamAvailabilityChecker sourceStreamAvailabilityChecker,
            IStreamCompletionChecker sourceStreamCompletionChecker)
        {
            if (sourceStream == null)
                throw new ArgumentNullException("sourceStream");

            if (targetStream == null)
                throw new ArgumentNullException("targetStream");

            if (buffer == null)
                throw new ArgumentNullException("buffer");

            _sourceStream = sourceStream;
            _targetStream = targetStream;
            _buffer = buffer;
            _sourceStreamAvailabilityChecker = sourceStreamAvailabilityChecker;
            _sourceStreamCompletionChecker = sourceStreamCompletionChecker;
        }

        public RepeatingTaskResult DoTask()
        {
            bool workDone = false;

            int dataAvailable = Buffer.Length - BufferWritePosition;

            if (SourceStreamAvailabilityChecker != null)
            {
                dataAvailable = Math.Min(SourceStreamAvailabilityChecker.DataAvailable.GetValueOrDefault(dataAvailable), dataAvailable);
            }

            if (dataAvailable > 0)
            {
                int read = SourceStream.Read(Buffer, BufferWritePosition, dataAvailable);

                workDone = (read > 0);

                BufferWritePosition += read;
            }

            if (BufferWritePosition > 0)
            {
                TargetStream.Write(Buffer, 0, BufferWritePosition);

                workDone = true;

                BufferWritePosition = 0;
            }

            bool workCompleted = false;

            if (SourceStreamCompletionChecker != null)
            {
                workCompleted = SourceStreamCompletionChecker.StreamCompleted;
            }

            return new RepeatingTaskResult(workDone, workCompleted);

        }

    }
}
