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
    public class PlaybackStreamTests
    {

        [TestMethod]
        public void PlaybackStreamReadsRecordedDataIfTimeOffsetPassed()
        {

            var currentDateTime = DateTime.Now;

            var currentDateTimeSourceMock = new Mock<ICurrentDateTimeSource>();

            currentDateTimeSourceMock.Setup(currentDateTimeSource =>
                currentDateTimeSource.GetCurrentDateTime()).Returns(() => currentDateTime);

            var memoryStream = new MemoryStream();

            var recordingStream = new RecordingStreamBuilder(memoryStream).SetCurrentDateTimeSource(currentDateTimeSourceMock.Object).Build();

            currentDateTime = currentDateTime.Add(TimeSpan.FromSeconds(2));

            var pastData = new byte[] { 1, 2, 3, 4 };

            recordingStream.Write(pastData, 0, pastData.Length);

            currentDateTime = currentDateTime.Add(TimeSpan.FromSeconds(3));

            var presentData = new byte[] { 5, 6, 7 };

            recordingStream.Write(presentData, 0, presentData.Length);

            currentDateTime = currentDateTime.Add(TimeSpan.FromSeconds(4));

            var futureData = new byte[] { 8, 9, 10, 11 };

            recordingStream.Write(futureData, 0, futureData.Length);

            memoryStream.Position = 0;

            currentDateTime = currentDateTime.Add(TimeSpan.FromSeconds(8));

            var playbackStream = new PlaybackStreamBuilder(memoryStream).SetCurrentDateTimeSource(currentDateTimeSourceMock.Object).Build();

            currentDateTime = currentDateTime.Add(TimeSpan.FromSeconds(5));

            var buffer = new byte[24];

            int read = playbackStream.Read(buffer, 0, buffer.Length);

            Assert.IsTrue(buffer.Take(read).SequenceEqual(pastData.Concat(presentData)));

        }

    }
}
