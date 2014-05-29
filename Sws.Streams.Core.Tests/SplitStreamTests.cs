using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sws.Streams.Core.Splitting;
using System.IO;
using Moq;

namespace Sws.Streams.Core.Tests
{

    [TestClass]
    public class SplitStreamTests
    {
 
        [TestMethod]
        public void SplitStreamWritesToAllTargetStreams()
        {
            var stream1 = new MemoryStream();

            var stream2 = new MemoryStream();

            var splitStream = new SplitStreamBuilder()
                .SetTargetStreams(new Stream[] { stream1 })
                .AddTargetStream(stream2)
                .Build();

            byte[] data = new byte[] { 1, 2, 3, 4 };

            splitStream.Write(data, 0, data.Length);

            Assert.IsTrue(data.SequenceEqual(stream1.ToArray()) && 
                data.SequenceEqual(stream2.ToArray()));
        }

        [TestMethod]
        public void SplitStreamWritesToOneTargetStreamIfAnotherErrorsAndExceptionHandlerSet()
        {
            var stream1 = new MemoryStream();

            var stream2Mock = new Mock<Stream>();

            stream2Mock.Setup(stream => stream.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception("Expected - Stream2 exception!"));

            var exceptionHandlerMock = new Mock<ISplitStreamExceptionHandler>();

            exceptionHandlerMock.Setup(exceptionHandler => exceptionHandler.HandleException(It.IsAny<Stream>(), It.IsAny<Exception>()));

            var splitStream = new SplitStreamBuilder()
                .SetTargetStreams(new Stream[] { stream1 })
                .AddTargetStream(stream2Mock.Object)
                .SetExceptionHandler(exceptionHandlerMock.Object)
                .Build();

            byte[] data = new byte[] { 1, 2, 3, 4 };

            splitStream.Write(data, 0, data.Length);

            Assert.IsTrue(data.SequenceEqual(stream1.ToArray()));
        }

        [TestMethod]
        public void SplitStreamReportsExceptionIfStreamErrorsAndExceptionHandlerSet()
        {
            var stream1 = new MemoryStream();

            var stream2Mock = new Mock<Stream>();

            var exception = new Exception("Expected - Stream2 exception!");

            stream2Mock.Setup(stream => stream.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(exception);

            var exceptionHandlerMock = new Mock<ISplitStreamExceptionHandler>();

            var exceptionCount = 0;

            exceptionHandlerMock.Setup(exceptionHandler => exceptionHandler.HandleException(stream2Mock.Object, exception))
                .Callback(() => exceptionCount++);

            var splitStream = new SplitStreamBuilder()
                .SetTargetStreams(new Stream[] { stream1 })
                .AddTargetStream(stream2Mock.Object)
                .SetExceptionHandler(exceptionHandlerMock.Object)
                .Build();

            byte[] data = new byte[] { 1, 2, 3, 4 };

            splitStream.Write(data, 0, data.Length);

            Assert.AreEqual(1, exceptionCount);

        }

        [TestMethod]
        public void SplitStreamThrowsExceptionIfStreamErrorsAndExceptionHandlerNotSet()
        {
            var stream1 = new MemoryStream();

            var stream2Mock = new Mock<Stream>();

            var exception = new Exception("Expected - Stream2 exception!");

            stream2Mock.Setup(stream => stream.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(exception);

            var splitStream = new SplitStreamBuilder()
                .SetTargetStreams(new Stream[] { stream1 })
                .AddTargetStream(stream2Mock.Object)
                .SetExceptionHandler(null)
                .Build();

            byte[] data = new byte[] { 1, 2, 3, 4 };

            Exception thrownException = null;

            try
            {
                splitStream.Write(data, 0, data.Length);
            }
            catch (Exception caughtException)
            {
                thrownException = caughtException;
            }

            Assert.AreEqual(exception, thrownException);

        }

    }
}
