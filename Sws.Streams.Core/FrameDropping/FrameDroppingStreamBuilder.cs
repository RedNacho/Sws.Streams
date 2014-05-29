using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Rewinding;
using Sws.Streams.Core.FrameDropping.Internal;

namespace Sws.Streams.Core.FrameDropping
{

    public class FrameDroppingStreamBuilder
    {

        private IRewindable _rewindable;

        public IRewindable Rewindable { get { return _rewindable; } }

        private IFrameFinder _frameFinder;

        public IFrameFinder FrameFinder { get { return _frameFinder; } }

        public FrameDroppingStreamBuilder(IRewindable rewindable, IFrameFinder frameFinder)
        {
            if (rewindable == null)
                throw new ArgumentNullException("rewindable");

            if (frameFinder == null)
                throw new ArgumentNullException("frameFinder");

            _rewindable = rewindable;

            _frameFinder = frameFinder;
        }

        public FrameDroppingStreamBuilder SetRewindable(IRewindable value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _rewindable = value;

            return this;
        }

        public FrameDroppingStreamBuilder SetFrameFinder(IFrameFinder value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _frameFinder = value;

            return this;
        }

        public Stream Build()
        {
            return new FrameDroppingStream(Rewindable, FrameFinder);
        }

    }

}
