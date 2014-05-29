using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Rewinding;
using Sws.Streams.Core.Common.AbstractStreamImplementations;
using Sws.Streams.Core.Common.Internal;

namespace Sws.Streams.Core.FrameDropping.Internal
{

    internal class FrameDroppingStream : NonSeekableReadOnlyStream
    {

        private readonly IRewindable _rewindable;

        public IRewindable Rewindable { get { return _rewindable; } }

        private readonly IFrameFinder _frameFinder;

        public IFrameFinder FrameFinder { get { return _frameFinder; } }

        private long? RemainingInFrame { get; set; }

        private readonly object _readSyncObject = new object();

        private object ReadSyncObject { get { return _readSyncObject; } }

        public FrameDroppingStream(IRewindable rewindable, IFrameFinder frameFinder)
        {
            _rewindable = rewindable;

            _frameFinder = frameFinder;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
                throw new IndexOutOfRangeException(ExceptionMessages.OffsetPlusCountGreaterThanBufferSizeMessage);

            lock (ReadSyncObject)
            {

                int totalRead = 0;

                int lastRead;

                bool movedToNextFrame = false;

                do
                {

                    if (RemainingInFrame.GetValueOrDefault() == 0 && (!movedToNextFrame))
                    {
                        PositionForNextFrame();

                        movedToNextFrame = true;
                    }

                    lastRead = ReadFromFrame(buffer, offset + totalRead, count - totalRead);

                    totalRead += lastRead;

                }
                while (lastRead > 0 && totalRead < count && RemainingInFrame.HasValue);

                return totalRead;

            }

        }

        private int ReadFromFrame(byte[] buffer, int offset, int count)
        {
            int lastRead = 0;

            if (RemainingInFrame.GetValueOrDefault() > 0)
            {
                int amountToRead = (int)Math.Min(RemainingInFrame.GetValueOrDefault(), count);

                lastRead = Rewindable.Stream.Read(buffer, offset, amountToRead);
                
                RemainingInFrame = RemainingInFrame.GetValueOrDefault() - lastRead;
            }

            return lastRead;
        }

        private void PositionForNextFrame()
        {
            var positionRecorder = new RewindablePositionRecorder();

            positionRecorder.Register(Rewindable);

            FrameSearchResult frameSearchResult;

            try
            {
                frameSearchResult = FrameFinder.ScanForCompleteFrames(Rewindable.Stream) ?? new FrameSearchResult();
            }
            finally
            {
                positionRecorder.Unregister(Rewindable);
            }

            var frameDescriptions = frameSearchResult.FramesFound ?? Enumerable.Empty<FrameDescription>();

            var frameDescription = frameDescriptions.OrderByDescending(frame => frame.RelativePosition)
                .FirstOrDefault();

            RemainingInFrame = null;

            if (frameDescription != null)
            {
                RemainingInFrame = frameDescription.FrameLength;

                Rewindable.Rewind(frameSearchResult.FinalRelativePosition - frameDescription.RelativePosition);
            }
            else
            {
                Rewindable.Rewind(positionRecorder.RelativePosition);
            }

            Rewindable.Truncate(0);

        }

    }

}
