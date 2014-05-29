using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Moq;
using Sws.Streams.Core.Common;
using Sws.Streams.Core.Recording;

namespace Sws.Streams.Core.Tests
{
    [TestClass]
    public class RecordingStreamTests
    {

        [TestMethod]
        public void RecordingStreamWritesTimeOffsetLengthAndDataToTargetStream()
        {

            var targetStream = new MemoryStream();

            var currentDateTimeSourceMock = new Mock<ICurrentDateTimeSource>();

            var initialDateTime = DateTime.Now;

            var currentDateTime = initialDateTime;

            currentDateTimeSourceMock.Setup(currentDateTimeSource => currentDateTimeSource.GetCurrentDateTime())
                .Returns(() => currentDateTime);

            var recordingStream = new RecordingStreamBuilder(targetStream).SetCurrentDateTimeSource(currentDateTimeSourceMock.Object).Build();

            var timeOffset = TimeSpan.FromSeconds(3);

            currentDateTime = initialDateTime.Add(timeOffset);

            var data = new byte[] { 1, 2, 3, 4, 5 };

            recordingStream.Write(data, 0, data.Length);

            var timeOffset2 = TimeSpan.FromSeconds(7);

            currentDateTime = initialDateTime.Add(timeOffset2);

            var data2 = new byte[] { 6, 7, 8, 9, 10, 11 };

            recordingStream.Write(data2, 0, data2.Length);

            Assert.IsTrue(targetStream.ToArray().SequenceEqual(
                BitConverter.GetBytes(timeOffset.Ticks).Concat(BitConverter.GetBytes(data.Length)).Concat(data)
                .Concat(BitConverter.GetBytes(timeOffset2.Ticks).Concat(BitConverter.GetBytes(data2.Length)).Concat(data2))));

        }

    }
}
