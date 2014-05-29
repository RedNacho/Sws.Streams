using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sws.Streams.Core.Rewinding;

namespace Sws.Streams.Core.Adapters
{
    public class RewindableStream : StreamWrapperBase
    {

        private readonly IRewindable _rewindable;

        public IRewindable Rewindable { get { return _rewindable; } }

        public RewindableStream(IRewindable rewindable, bool disposeOfRewindableOnDispose)
            : base(rewindable.Stream, rewindable.Stream, rewindable.Stream, GetDisposables(rewindable, disposeOfRewindableOnDispose))
        {
            _rewindable = rewindable;

            _rewindable.PositionIncremented += RewindablePositionIncremented;

            _rewindable.PositionDecremented += RewindablePositionDecremented;
        }

        private static IDisposable[] GetDisposables(IRewindable rewindable, bool disposeOfRewindableOnDispose)
        {
            if (disposeOfRewindableOnDispose)
            {
                return new IDisposable[] { rewindable };
            }

            return null;
        }

        public void Rewind(long step)
        {
            Rewindable.Rewind(step);
        }

        public void Truncate(long maximumRewindStep)
        {
            Rewindable.Truncate(maximumRewindStep);
        }

        public event PositionChangedEventHandler PositionIncremented;

        public event PositionChangedEventHandler PositionDecremented;

        private void RewindablePositionIncremented(object sender, PositionChangedEventArgs e)
        {
            if (PositionIncremented != null)
            {
                PositionIncremented(this, e);
            }
        }

        private void RewindablePositionDecremented(object sender, PositionChangedEventArgs e)
        {
            if (PositionDecremented != null)
            {
                PositionDecremented(this, e);
            }
        }

    }
}
