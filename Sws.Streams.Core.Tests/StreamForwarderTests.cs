using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sws.Streams.Core.Forwarding;
using Sws.Streams.Core.Common;
using System.IO;
using Moq;

namespace Sws.Streams.Core.Tests
{

    [TestClass]
    public class StreamForwarderTests
    {

        [TestMethod]
        public void SourceStreamCompletionCheckerTerminatesStart()
        {

            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var sourceStream = new MemoryStream();

            var targetStream = new MemoryStream();

            sourceStream.Write(data, 0, data.Length);

            sourceStream.Position = 0;

            var sourceStreamCompletionCheckerMock = new Mock<IStreamCompletionChecker>();

            sourceStreamCompletionCheckerMock.Setup(completionChecker => completionChecker.StreamCompleted)
                .Returns(() => sourceStream.Position >= 5);

            var streamForwarder = new StreamForwarderBuilder(sourceStream, targetStream, 1)
                .SetSourceStreamCompletionChecker(sourceStreamCompletionCheckerMock.Object).Build();

            streamForwarder.Run();

            Assert.AreEqual(5, sourceStream.Position);
        }

        [TestMethod]
        public void DataIsForwardedFromSourceToTarget()
        {

            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var sourceStream = new MemoryStream();

            var targetStream = new MemoryStream();

            sourceStream.Write(data, 0, data.Length);

            sourceStream.Position = 0;

            var sourceStreamCompletionCheckerMock = new Mock<IStreamCompletionChecker>();

            sourceStreamCompletionCheckerMock.Setup(completionChecker => completionChecker.StreamCompleted)
                .Returns(() => sourceStream.Position >= data.Length);

            var streamForwarder = new StreamForwarderBuilder(sourceStream, targetStream, 6)
                .SetSourceStreamCompletionChecker(sourceStreamCompletionCheckerMock.Object).Build();

            streamForwarder.Run();

            Assert.IsTrue(targetStream.ToArray().SequenceEqual(data));

        }

        [TestMethod]
        public void IfNoDataReadAndPollIntervalSetThreadPaused()
        {
            var pollInterval = TimeSpan.FromSeconds(1);

            var sourceStreamMock = new Mock<Stream>();
            
            var sourceStreamCompletionCheckerMock = new Mock<IStreamCompletionChecker>();

            int callCount = 0;

            int zeroByteCalls = 2;

            sourceStreamMock.Setup(sourceStream => sourceStream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((Func<byte[], int, int, int>)((buffer, offset, count) =>
                    {
                        callCount++;

                        if (callCount <= zeroByteCalls)
                        {
                            return 0;
                        }
                        else
                        {
                            for (int index = 0; index < count; index++)
                            {
                                buffer[index + offset] = (byte)(index % 256);
                            }
                            return count;
                        }

                    }));

            sourceStreamCompletionCheckerMock.Setup(completionChecker => completionChecker.StreamCompleted)
                .Returns(() => callCount > zeroByteCalls);

            var targetStream = new MemoryStream();

            var threadPauserMock = new Mock<IThreadPauser>();

            int pauseCount = 0;

            threadPauserMock.Setup(threadPauser => threadPauser.Pause(pollInterval))
                .Callback(() => pauseCount++);

            var streamForwarder = new StreamForwarderBuilder(sourceStreamMock.Object, targetStream, 1024)
                .SetPollInterval(pollInterval).SetThreadPauser(threadPauserMock.Object)
                .SetSourceStreamCompletionChecker(sourceStreamCompletionCheckerMock.Object).Build();

            streamForwarder.Run();

            Assert.AreEqual(zeroByteCalls, pauseCount);

        }

        [TestMethod]
        public void IfNoDataReadAndPollIntervalNotSetThreadNotPaused()
        {
            var sourceStreamMock = new Mock<Stream>();

            var sourceStreamCompletionCheckerMock = new Mock<IStreamCompletionChecker>();

            int callCount = 0;

            int zeroByteCalls = 2;

            sourceStreamMock.Setup(sourceStream => sourceStream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((Func<byte[], int, int, int>)((buffer, offset, count) =>
                {
                    callCount++;

                    if (callCount <= zeroByteCalls)
                    {
                        return 0;
                    }
                    else
                    {
                        for (int index = 0; index < count; index++)
                        {
                            buffer[index + offset] = (byte)(index % 256);
                        }
                        return count;
                    }

                }));

            sourceStreamCompletionCheckerMock.Setup(completionChecker => completionChecker.StreamCompleted)
                .Returns(() => callCount > zeroByteCalls);

            var targetStream = new MemoryStream();

            var threadPauserMock = new Mock<IThreadPauser>();

            int pauseCount = 0;

            threadPauserMock.Setup(threadPauser => threadPauser.Pause(It.IsAny<TimeSpan>()))
                .Callback(() => pauseCount++);

            var streamForwarder = new StreamForwarderBuilder(sourceStreamMock.Object, targetStream, 1024)
                .SetPollInterval(null).SetThreadPauser(threadPauserMock.Object)
                .SetSourceStreamCompletionChecker(sourceStreamCompletionCheckerMock.Object).Build();

            streamForwarder.Run();

            Assert.AreEqual(0, pauseCount);

        }

        [TestMethod]
        public void ReadNotCalledIfSourceStreamAvailabilityCheckerReturnsZero()
        {

            var sourceStreamMock = new Mock<Stream>();

            var readCount = 0;

            sourceStreamMock.Setup(sourceStream => sourceStream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => { readCount++; return 0; });

            var availabilityCheckCount = 0;

            var sourceStreamAvailabilityCheckerMock = new Mock<IStreamAvailabilityChecker>();

            sourceStreamAvailabilityCheckerMock.Setup(sourceStreamAvailabilityChecker => sourceStreamAvailabilityChecker.DataAvailable)
                .Returns(() => { availabilityCheckCount++; return 0; });

            var sourceStreamCompletionCheckerMock = new Mock<IStreamCompletionChecker>();

            sourceStreamCompletionCheckerMock.Setup(sourceStreamCompletionChecker => sourceStreamCompletionChecker.StreamCompleted)
                .Returns(() => availabilityCheckCount >= 10);

            var targetStream = new MemoryStream();

            var streamForwarder = new StreamForwarderBuilder(sourceStreamMock.Object, targetStream, 1024)
                .SetSourceStreamAvailabilityChecker(sourceStreamAvailabilityCheckerMock.Object)
                .SetSourceStreamCompletionChecker(sourceStreamCompletionCheckerMock.Object).Build();

            streamForwarder.Run();

            Assert.AreEqual(0, readCount);

        }

        [TestMethod]
        public void IfDataReadAndPollIntervalSetThreadNotPaused()
        {
            var pollInterval = TimeSpan.FromSeconds(1);

            var sourceStreamMock = new Mock<Stream>();

            var sourceStreamCompletionCheckerMock = new Mock<IStreamCompletionChecker>();

            int callCount = 0;

            int totalCalls = 3;

            sourceStreamMock.Setup(sourceStream => sourceStream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((Func<byte[], int, int, int>)((buffer, offset, count) =>
                {
                    callCount++;

                    for (int index = 0; index < count; index++)
                    {
                        buffer[index + offset] = (byte)(index % 256);
                    }
                    return count;

                }));

            sourceStreamCompletionCheckerMock.Setup(completionChecker => completionChecker.StreamCompleted)
                .Returns(() => callCount >= totalCalls);

            var targetStream = new MemoryStream();

            var threadPauserMock = new Mock<IThreadPauser>();

            int pauseCount = 0;

            threadPauserMock.Setup(threadPauser => threadPauser.Pause(pollInterval))
                .Callback(() => pauseCount++);

            var streamForwarder = new StreamForwarderBuilder(sourceStreamMock.Object, targetStream, 1024)
                .SetPollInterval(pollInterval).SetThreadPauser(threadPauserMock.Object)
                .SetSourceStreamCompletionChecker(sourceStreamCompletionCheckerMock.Object).Build();

            streamForwarder.Run();

            Assert.AreEqual(0, pauseCount);

        }

        [TestMethod]
        public void IfErrorOccursAndNoExceptionHandlerSetupThenExceptionIsThrown()
        {

            var sourceStreamMock = new Mock<Stream>();

            var expectedException = new Exception("Error reading Stream");

            sourceStreamMock.Setup(sourceStream => sourceStream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(expectedException);

            var targetStream = new MemoryStream();

            var streamForwarder = new StreamForwarderBuilder(sourceStreamMock.Object, targetStream, 1024).Build();

            Exception thrownException = null;

            try
            {
                streamForwarder.Run();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.AreEqual(thrownException, expectedException);

        }

        [TestMethod]
        public void IfErrorOccursAndExceptionHandlerSetupThenExceptionIsReported()
        {

            var sourceStreamMock = new Mock<Stream>();

            var expectedException = new Exception("Error reading Stream");

            sourceStreamMock.Setup(sourceStream => sourceStream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(expectedException);

            var targetStream = new MemoryStream();

            int exceptionReportCount = 0;

            var exceptionHandlerMock = new Mock<IExceptionHandler>();
            
            var stopAfterExceptionCount = 3;

            IStreamForwarder streamForwarder = null;

            exceptionHandlerMock.Setup(exceptionHandler => exceptionHandler.HandleException(expectedException))
                .Callback(() =>
                    {
                        exceptionReportCount++;

                        if (exceptionReportCount == stopAfterExceptionCount)
                        {
                            streamForwarder.Stop();
                        }
                    }
                );

            streamForwarder = new StreamForwarderBuilder(sourceStreamMock.Object, targetStream, 1024)
                .SetExceptionHandler(exceptionHandlerMock.Object)
                .Build();

            streamForwarder.Run();

            Assert.AreEqual(exceptionReportCount, stopAfterExceptionCount);

        }

        [TestMethod]
        public void IfErrorOccursDuringWriteAndExceptionHandlerSetupThenWriteIsRetried()
        {

            var sourceStream = new MemoryStream();

            byte[] data = new byte[] { 1, 2, 3, 4, 5 };

            sourceStream.Write(data, 0, data.Length);

            sourceStream.Position = 0;

            var expectedException = new Exception("Error writing Stream");

            var exceptionOccurred = false;

            var targetStreamMock = new Mock<Stream>();

            var targetInnerStream = new MemoryStream();

            targetStreamMock.Setup(targetStream => targetStream.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback((Action<byte[], int, int>)((buffer, offset, count) =>
                {
                    if (!exceptionOccurred)
                    {
                        exceptionOccurred = true;
                        throw expectedException;
                    }
                    else
                    {
                        targetInnerStream.Write(buffer, offset, count);
                    }
                }));

            var exceptionHandlerMock = new Mock<IExceptionHandler>();

            IStreamForwarder streamForwarder = null;

            exceptionHandlerMock.Setup(exceptionHandler => exceptionHandler.HandleException(expectedException))
                .Callback(() => { });

            var sourceStreamCompletionCheckerMock = new Mock<IStreamCompletionChecker>();

            sourceStreamCompletionCheckerMock.Setup(
                    sourceStreamCompletionChecker => sourceStreamCompletionChecker.StreamCompleted
                )
                .Returns(() => sourceStream.Position == sourceStream.Length);

            streamForwarder = new StreamForwarderBuilder(sourceStream, targetStreamMock.Object, 1024)
                .SetExceptionHandler(exceptionHandlerMock.Object)
                .SetSourceStreamCompletionChecker(sourceStreamCompletionCheckerMock.Object)
                .Build();

            streamForwarder.Run();

            Assert.IsTrue(targetInnerStream.ToArray().SequenceEqual(data));

        }

    }
}
