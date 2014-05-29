using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Forwarding.Internal;
using Sws.Streams.Core.Common;
using Sws.Streams.Core.Common.Internal;

namespace Sws.Streams.Core.Forwarding
{
    public class StreamForwarderBuilder
    {

        private Stream _sourceStream;

        public Stream SourceStream { get { return _sourceStream; } }

        private Stream _targetStream;

        public Stream TargetStream { get { return _targetStream; } }

        private int _bufferSize;

        public int BufferSize { get { return _bufferSize; } }

        private TimeSpan? _pollInterval = null;

        public TimeSpan? PollInterval { get { return _pollInterval; } }

        private IExceptionHandler _exceptionHandler = null;

        public IExceptionHandler ExceptionHandler { get { return _exceptionHandler; } }

        private IStreamAvailabilityChecker _sourceStreamAvailabilityChecker = null;

        public IStreamAvailabilityChecker SourceStreamAvailabilityChecker { get { return _sourceStreamAvailabilityChecker; } }

        private IStreamCompletionChecker _sourceStreamCompletionChecker = null;

        public IStreamCompletionChecker SourceStreamCompletionChecker { get { return _sourceStreamCompletionChecker; } }

        private IThreadPauser _threadPauser = new ThreadPauser();

        public IThreadPauser ThreadPauser { get { return _threadPauser; } }

        public StreamForwarderBuilder(Stream sourceStream, Stream targetStream, int bufferSize)
        {
            if (sourceStream == null)
                throw new ArgumentNullException("sourceStream");

            if (targetStream == null)
                throw new ArgumentNullException("targetStream");

            if (bufferSize <= 0)
                throw new ArgumentException("bufferSize must be greater than zero.", "bufferSize");

            _sourceStream = sourceStream;
            _targetStream = targetStream;
            _bufferSize = bufferSize;
        }

        public StreamForwarderBuilder SetSourceStream(Stream value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _sourceStream = value;

            return this;
        }

        public StreamForwarderBuilder SetTargetStream(Stream value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _targetStream = value;

            return this;
        }

        public StreamForwarderBuilder SetBufferSize(int value)
        {
            if (value <= 0)
                throw new ArgumentException(string.Format(ExceptionMessages.ValueMustBeGreaterThanZeroFormat, "value"), "value");

            _bufferSize = value;

            return this;
        }

        public StreamForwarderBuilder SetPollInterval(TimeSpan? value)
        {
            _pollInterval = value;

            return this;
        }

        public StreamForwarderBuilder SetExceptionHandler(IExceptionHandler value)
        {
            _exceptionHandler = value;

            return this;
        }

        public StreamForwarderBuilder SetSourceStreamAvailabilityChecker(IStreamAvailabilityChecker value)
        {
            _sourceStreamAvailabilityChecker = value;

            return this;
        }

        public StreamForwarderBuilder SetSourceStreamCompletionChecker(IStreamCompletionChecker value)
        {
            _sourceStreamCompletionChecker = value;

            return this;
        }

        public StreamForwarderBuilder SetThreadPauser(IThreadPauser value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _threadPauser = value;

            return this;
        }

        public IStreamForwarder Build()
        {
            return new StreamForwarder(SourceStream, TargetStream, BufferSize, SourceStreamAvailabilityChecker,
                SourceStreamCompletionChecker, new InterruptibleRepeater(PollInterval, ThreadPauser, ExceptionHandler));
        }

    }
}
