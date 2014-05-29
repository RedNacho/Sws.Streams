using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Splitting.Internal;

namespace Sws.Streams.Core.Splitting
{
    public class SplitStreamBuilder
    {

        private ISplitStreamExceptionHandler _exceptionHandler = null;

        public ISplitStreamExceptionHandler ExceptionHandler { get { return _exceptionHandler; } }

        private IEnumerable<Stream> _targetStreams = null;

        public IEnumerable<Stream> TargetStreams { get { return _targetStreams; } }

        public SplitStreamBuilder SetTargetStreams(IEnumerable<Stream> targetStreams)
        {
            _targetStreams = targetStreams;

            return this;
        }

        public SplitStreamBuilder AddTargetStream(Stream targetStream)
        {
            var targetStreams = _targetStreams;

            if (targetStreams == null)
            {
                targetStreams = Enumerable.Empty<Stream>();
            }

            IList<Stream> list = targetStreams.ToList();

            if (!list.Contains(targetStream))
            {
                list.Add(targetStream);
            }

            _targetStreams = list;

            return this;
        }

        public SplitStreamBuilder SetExceptionHandler(ISplitStreamExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;

            return this;
        }

        public Stream Build()
        {
            return new SplitStream(TargetStreams, ExceptionHandler);
        }

    }
}
