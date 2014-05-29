using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Common;
using Sws.Streams.Core.Recording.Internal;

namespace Sws.Streams.Core.Recording
{
    public class PlaybackStreamBuilder
    {

        private Stream _sourceStream;

        public Stream SourceStream { get { return _sourceStream; } }

        private ICurrentDateTimeSource _currentDateTimeSource = new CurrentDateTimeSource();

        public ICurrentDateTimeSource CurrentDateTimeSource { get { return _currentDateTimeSource; } }

        private DateTime? _constructionTimestamp = null;

        public DateTime? ConstructionTimestamp { get { return _constructionTimestamp; } }

        public PlaybackStreamBuilder(Stream sourceStream)
        {
            if (sourceStream == null)
                throw new ArgumentNullException("sourceStream");

            _sourceStream = sourceStream;
        }

        public PlaybackStreamBuilder SetSourceStream(Stream value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _sourceStream = value;

            return this;
        }

        public PlaybackStreamBuilder SetCurrentDateTimeSource(ICurrentDateTimeSource value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _currentDateTimeSource = value;

            return this;
        }

        public PlaybackStreamBuilder SetConstructionTimestamp(DateTime? value)
        {
            _constructionTimestamp = value;

            return this;
        }

        public Stream Build()
        {
            return new PlaybackStream(SourceStream, CurrentDateTimeSource, ConstructionTimestamp.GetValueOrDefault(CurrentDateTimeSource.GetCurrentDateTime()));
        }

    }
}

