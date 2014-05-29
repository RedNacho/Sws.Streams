using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rewinding.Internal
{

    internal class Rewindable : IRewindable
    {

        private readonly RewindableStream _rewindableStream;

        public RewindableStream RewindableStream { get { return _rewindableStream; } }

        public Rewindable(RewindableStream rewindableStream)
        {
            if (rewindableStream == null)
                throw new ArgumentNullException("rewindableStream");

            _rewindableStream = rewindableStream;

            _rewindableStream.PositionIncremented += StreamPositionIncremented;

            _rewindableStream.PositionDecremented += StreamPositionDecremented;
        }

        public Stream Stream
        {
            get { return RewindableStream; }
        }

        public void Rewind(long step)
        {
            RewindableStream.Rewind(step);
        }

        public void Truncate(long maximumRewindStep)
        {
            RewindableStream.Truncate(maximumRewindStep);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RewindableStream.Dispose();
            }
        }

        public event PositionChangedEventHandler PositionIncremented;

        public event PositionChangedEventHandler PositionDecremented;

        private void StreamPositionIncremented(object sender, PositionChangedEventArgs e)
        {
            if (PositionIncremented != null)
            {
                PositionIncremented(this, e);
            }
        }

        private void StreamPositionDecremented(object sender, PositionChangedEventArgs e)
        {
            if (PositionDecremented != null)
            {
                PositionDecremented(this, e);
            }
        }

    }

}
