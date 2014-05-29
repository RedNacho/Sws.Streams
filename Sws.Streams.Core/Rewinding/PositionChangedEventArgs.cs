using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Rewinding
{
    public class PositionChangedEventArgs : EventArgs
    {

        private readonly long _delta;

        public long Delta { get { return _delta; } }

        internal PositionChangedEventArgs(long delta)
        {
            _delta = delta;
        }

    }
}
