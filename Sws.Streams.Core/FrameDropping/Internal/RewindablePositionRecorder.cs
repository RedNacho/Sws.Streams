using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sws.Streams.Core.Rewinding;

namespace Sws.Streams.Core.FrameDropping.Internal
{

    internal class RewindablePositionRecorder
    {

        private long _relativePosition;

        public void Register(IRewindable rewindable)
        {
            rewindable.PositionIncremented += RewindablePositionIncremented;
            rewindable.PositionDecremented += RewindablePositionDecremented;
        }

        public void Unregister(IRewindable rewindable)
        {
            rewindable.PositionIncremented -= RewindablePositionIncremented;
            rewindable.PositionDecremented -= RewindablePositionDecremented;
        }

        private void RewindablePositionIncremented(object sender, PositionChangedEventArgs e)
        {
            _relativePosition += e.Delta;
        }

        private void RewindablePositionDecremented(object sender, PositionChangedEventArgs e)
        {
            _relativePosition -= e.Delta;
        }

        public long RelativePosition
        {
            get { return _relativePosition; }
        }

    }

}
