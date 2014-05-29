using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sws.Streams.Core.FrameDropping;
using Sws.Streams.Core.Rewinding;
using Moq;
using System.IO;

namespace Sws.Streams.Core.Tests
{

    [TestClass]
    public class FrameDroppingStreamTests
    {

        [TestMethod]
        public void FrameDroppingStreamDropsFramesWithStandardFrameFinder()
        {


            var data = new byte[] { 1, 2, 3, 4, 5, 6 };

            var memoryStream = new MemoryStream();

            memoryStream.Write(data, 0, data.Length);

            memoryStream.Position = 0;

            var rewindable = new RewindableBuilder(memoryStream).Build();

            var frameFinderMock = new Mock<IFrameFinder>();

            frameFinderMock.Setup(
                frameFinder => frameFinder.ScanForCompleteFrames(rewindable.Stream)
            ).Returns(
                () =>
                {
                    var buffer = new byte[1];
                    int read;
                    int totalRead = 0;

                    List<FrameDescription> frames = new List<FrameDescription>();
                    
                    do
                    {
                        read = rewindable.Stream.Read(buffer, 0, buffer.Length);

                        if (read > 0)
                        {

                            frames.Add(new FrameDescription() { RelativePosition = totalRead, FrameLength = read });

                            totalRead += read;

                        }

                    }
                    while (read > 0);

                    return new FrameSearchResult()
                    {
                        FinalRelativePosition = totalRead,
                        FramesFound = frames
                    };

                }
            );

            var frameDroppingStream = new FrameDroppingStreamBuilder(rewindable, frameFinderMock.Object).Build();

            var result = new byte[data.Length];

            int resultRead = frameDroppingStream.Read(result, 0, result.Length);

            Assert.IsTrue(result.Take(resultRead).SequenceEqual(data.Skip(data.Length - 1)));

        }


        [TestMethod]
        public void IfNoFramesAvailableRetriesFromSamePosition()
        {
            var sourceStreamMock = new Mock<Stream>();

            var data = new byte[] { 1, 2, 3, 4, 5, 6 };

            int readCallCount = 0;

            sourceStreamMock.Setup(sourceStream => sourceStream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((Func<byte[], int, int, int>)((buffer, offset, count) =>
                {
                    int read = 0;

                    if (readCallCount < data.Length)
                    {
                        buffer[offset] = data[readCallCount];
                        read = 1;
                    }

                    readCallCount++;

                    return read;

                }));

            var rewindable = new RewindableBuilder(sourceStreamMock.Object).Build();

            var frameFinderMock = new Mock<IFrameFinder>();

            frameFinderMock.Setup(frameFinder => frameFinder.ScanForCompleteFrames(rewindable.Stream))
                .Returns(
                    () =>
                    {
                        byte[] buffer = new byte[3];

                        int read = rewindable.Stream.Read(buffer, 0, buffer.Length);

                        var frameDescriptions = new List<FrameDescription>();

                        if (read == buffer.Length)
                        {
                            frameDescriptions.Add(new FrameDescription() { RelativePosition = -3, FrameLength = 3 });
                        }

                        return new FrameSearchResult() { FinalRelativePosition = 0, FramesFound = frameDescriptions };

                    }
                );

            var frameDroppingStream = new FrameDroppingStreamBuilder(rewindable, frameFinderMock.Object).Build();

            var result = new byte[data.Length];

            int totalResultRead = 0;
            
            int resultRead;

            int readAttempts = 0;

            do
            {
                resultRead = frameDroppingStream.Read(result, totalResultRead, result.Length - totalResultRead);
                totalResultRead += resultRead;
                readAttempts++;
            }
            while (totalResultRead <= data.Length && readAttempts <= data.Length);

            Assert.IsTrue(result.Take(totalResultRead).SequenceEqual(data));

        }

        [TestMethod]
        public void FrameDroppingStreamPreservesOriginalStreamForFirstFrameOnlyFrameFinder()
        {

            var data = new byte[] { 1, 2, 3, 4, 5, 6 };

            var memoryStream = new MemoryStream();

            memoryStream.Write(data, 0, data.Length);

            memoryStream.Position = 0;

            var rewindable = new RewindableBuilder(memoryStream).Build();

            var frameFinderMock = new Mock<IFrameFinder>();

            frameFinderMock.Setup(
                frameFinder => frameFinder.ScanForCompleteFrames(rewindable.Stream)
            ).Returns(
                () =>
                {
                    var buffer = new byte[1];
                    int read = rewindable.Stream.Read(buffer, 0, buffer.Length);

                    if (read > 0)
                    {
                        return new FrameSearchResult
                        {
                            FinalRelativePosition = read,
                            FramesFound = new [] { new FrameDescription() { RelativePosition = 0, FrameLength = read } }
                        };
                    }

                    return new FrameSearchResult();

                }
            );

            var frameDroppingStream = new FrameDroppingStreamBuilder(rewindable, frameFinderMock.Object).Build();

            var result = new byte[data.Length];

            int resultRead = 0;

            while (resultRead < data.Length)
            {

                resultRead += frameDroppingStream.Read(result, resultRead, result.Length - resultRead);

            }

            Assert.IsTrue(result.Take(resultRead).SequenceEqual(data));

        }

    }
}
