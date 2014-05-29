using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common;
using Sws.Streams.Core.Recording.Internal;

namespace Sws.Streams.Core.Recording
{
    public class RecordingStreamBuilder
    {

        private Stream _targetStream;

        public Stream TargetStream { get { return _targetStream; } }

        private ICurrentDateTimeSource _currentDateTimeSource = new CurrentDateTimeSource();

        public ICurrentDateTimeSource CurrentDateTimeSource { get { return _currentDateTimeSource; } }

        private DateTime? _constructionTimestamp = null;

        public DateTime? ConstructionTimestamp { get { return _constructionTimestamp; } }

        public RecordingStreamBuilder(Stream targetStream)
        {
            if (targetStream == null)
                throw new ArgumentNullException("targetStream");

            _targetStream = targetStream;
        }

        public RecordingStreamBuilder SetTargetStream(Stream value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _targetStream = value;

            return this;
        }

        public RecordingStreamBuilder SetCurrentDateTimeSource(ICurrentDateTimeSource value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _currentDateTimeSource = value;

            return this;
        }

        public RecordingStreamBuilder SetConstructionTimestamp(DateTime? value)
        {
            _constructionTimestamp = value;

            return this;
        }

        public Stream Build()
        {
            return new RecordingStream(TargetStream, CurrentDateTimeSource, ConstructionTimestamp.GetValueOrDefault(CurrentDateTimeSource.GetCurrentDateTime()));
        }

    }
}
