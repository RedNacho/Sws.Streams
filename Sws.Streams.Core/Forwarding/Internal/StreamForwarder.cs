using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Sws.Streams.Core.Common;
using Sws.Streams.Core.Common.Internal;

namespace Sws.Streams.Core.Forwarding.Internal
{

    internal class StreamForwarder : IStreamForwarder
    {

        private readonly Stream _sourceStream;

        public Stream SourceStream { get { return _sourceStream; } }

        private readonly Stream _targetStream;

        public Stream TargetStream { get { return _targetStream; } }

        private readonly int _bufferSize;

        public int BufferSize { get { return _bufferSize; } }

        private readonly IStreamAvailabilityChecker _sourceStreamAvailabilityChecker;

        public IStreamAvailabilityChecker SourceStreamAvailabilityChecker { get { return _sourceStreamAvailabilityChecker; } }

        private readonly IStreamCompletionChecker _sourceStreamCompletionChecker;

        public IStreamCompletionChecker SourceStreamCompletionChecker { get { return _sourceStreamCompletionChecker; } }

        private readonly IInterruptibleRepeater _interruptibleRepeater;

        public IInterruptibleRepeater InterruptibleRepeater { get { return _interruptibleRepeater; } }

        private readonly byte[] _buffer;

        private byte[] Buffer { get { return _buffer; } }

        public StreamForwarder(Stream sourceStream, Stream targetStream, int bufferSize, 
            IStreamAvailabilityChecker sourceStreamAvailabilityChecker,
            IStreamCompletionChecker sourceStreamCompletionChecker,
            IInterruptibleRepeater interruptibleRepeater
        )
        {
            if (sourceStream == null)
                throw new ArgumentNullException("sourceStream");

            if (targetStream == null)
                throw new ArgumentNullException("targetStream");

            if (interruptibleRepeater == null)
                throw new ArgumentNullException("interruptibleRepeater");

            _sourceStream = sourceStream;
            _targetStream = targetStream;
            _bufferSize = bufferSize;
            _interruptibleRepeater = interruptibleRepeater;
            _sourceStreamAvailabilityChecker = sourceStreamAvailabilityChecker;
            _sourceStreamCompletionChecker = sourceStreamCompletionChecker;

            _buffer = new byte[bufferSize];
        }

        public bool IsRunning
        {
            get
            {
                return InterruptibleRepeater.IsRunning;
            }
        }

        public void Run()
        {
            InterruptibleRepeater.Run(new StreamForwarderRepeatingTask(SourceStream, TargetStream, Buffer, SourceStreamAvailabilityChecker, SourceStreamCompletionChecker));
        }

        public void Stop()
        {
            InterruptibleRepeater.Stop();
        }

    }

}
