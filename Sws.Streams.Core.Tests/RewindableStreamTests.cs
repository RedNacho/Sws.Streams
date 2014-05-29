using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Sws.Streams.Core.Rewinding;
using Sws.Streams.Core.Rewinding.MemoryStreamBased;
using Moq;

namespace Sws.Streams.Core.Tests
{

    [TestClass]
    public class RewindableStreamTests
    {
        
        [TestMethod]
        public void RewindableStreamWrapsSourceStream()
        {
            var sourceStream = new MemoryStream();

            var sourceData = new byte[] { 1, 2, 3, 4 };

            sourceStream.Write(sourceData, 0, sourceData.Length);

            sourceStream.Position = 0;

            var rewindable = new RewindableBuilder(sourceStream).Build();

            var outputData = new byte[sourceData.Length];

            rewindable.Stream.Read(outputData, 0, outputData.Length);

            Assert.IsTrue(outputData.SequenceEqual(sourceData));
        }

        [TestMethod]
        public void RewindableStreamRewindsAndAllowsDataToBeReread()
        {
            var sourceStream = new MemoryStream();

            var sourceData = new byte[] { 1, 2, 3, 4 };

            sourceStream.Write(sourceData, 0, sourceData.Length);

            sourceStream.Position = 0;

            var rewindable = new RewindableBuilder(sourceStream).Build();

            var outputData = new byte[sourceData.Length];

            rewindable.Stream.Read(outputData, 0, outputData.Length);

            rewindable.Rewind(outputData.Length);

            var repeatedData = new byte[sourceData.Length];

            rewindable.Stream.Read(repeatedData, 0, repeatedData.Length);

            Assert.IsTrue(repeatedData.SequenceEqual(sourceData));
        }

        [TestMethod]
        public void RewindableStreamRewindsWithoutModifyingPositionOfSourceStream()
        {
            var sourceStream = new MemoryStream();

            var sourceData = new byte[] { 1, 2, 3, 4 };

            sourceStream.Write(sourceData, 0, sourceData.Length);

            sourceStream.Position = 0;

            var rewindable = new RewindableBuilder(sourceStream).Build();

            var outputData = new byte[sourceData.Length];

            rewindable.Stream.Read(outputData, 0, outputData.Length);

            var positionBeforeRewind = sourceStream.Position;

            rewindable.Rewind(outputData.Length);

            var positionAfterRewind = sourceStream.Position;

            Assert.AreEqual(positionBeforeRewind, positionAfterRewind);
        }

        [TestMethod]
        public void CacheTruncatedOnTruncateCall()
        {
            var sourceStream = new MemoryStream();

            var sourceData = new byte[] { 1, 2, 3, 4, 5, 6 };

            sourceStream.Write(sourceData, 0, sourceData.Length);

            sourceStream.Position = 0;

            var memoryStreamCacheAccessor = new MemoryStreamCacheAccessor();

            var rewindable = new RewindableBuilder(sourceStream).SetCacheAccessor(memoryStreamCacheAccessor).Build();

            var outputData = new byte[sourceData.Length];

            rewindable.Stream.Read(outputData, 0, outputData.Length);

            rewindable.Truncate(1);

            Assert.IsTrue(memoryStreamCacheAccessor.Stream.ToArray().SequenceEqual(new byte[] { 6 }));
        }

        [TestMethod]
        public void CacheTruncatedToAccommodateNewDataWhenAutoTruncateAtBytesPassed()
        {
            var sourceStream = new MemoryStream();

            var sourceData = new byte[] { 1, 2, 3, 4, 5, 6 };

            sourceStream.Write(sourceData, 0, sourceData.Length);

            sourceStream.Position = 0;

            var memoryStreamCacheAccessor = new MemoryStreamCacheAccessor();

            var rewindable = new RewindableBuilder(sourceStream).SetCacheAccessor(memoryStreamCacheAccessor)
                .SetAutoTruncateAtBytes(2).Build();

            var outputData = new byte[sourceData.Length];

            rewindable.Stream.Read(outputData, 0, outputData.Length);

            Assert.IsTrue(memoryStreamCacheAccessor.Stream.ToArray().SequenceEqual(new byte[] { 5, 6 }));
        }

        [TestMethod]
        public void CacheTruncatedToAutoTruncationTailLengthToAccommodateNewDataWhenAutoTruncateAtBytesPassed()
        {
            var sourceStream = new MemoryStream();

            var sourceData = new byte[] { 1, 2, 3, 4, 5, 6 };

            sourceStream.Write(sourceData, 0, sourceData.Length);

            sourceStream.Position = 0;

            var memoryStreamCacheAccessor = new MemoryStreamCacheAccessor();

            var rewindable = new RewindableBuilder(sourceStream).SetCacheAccessor(memoryStreamCacheAccessor)
                .SetAutoTruncateAtBytes(4).SetAutoTruncationTailLength(0).Build();

            var outputData = new byte[sourceData.Length];

            rewindable.Stream.Read(outputData, 0, outputData.Length);

            Assert.IsTrue(memoryStreamCacheAccessor.Stream.ToArray().SequenceEqual(new byte[] { 5, 6 }));
        }

    }
}
