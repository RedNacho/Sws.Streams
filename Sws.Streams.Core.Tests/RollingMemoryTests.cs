using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sws.Streams.Core.Rolling;
using Moq;
using System.IO;
using Sws.Streams.Core.Common;

namespace Sws.Streams.Core.Tests
{
    [TestClass]
    public class RollingMemoryTests
    {

        private IWriteStreamFactory CreateMemoryWriteStreamFactory(List<Stream> createdStreamRegister)
        {
            var writeStreamFactoryMock = new Mock<IWriteStreamFactory>();

            writeStreamFactoryMock.Setup(writeStreamFactory => writeStreamFactory.CreateWriteStream())
                .Returns(() =>
                    {
                        var output = new MemoryStream();

                        if (createdStreamRegister != null)
                        {
                            createdStreamRegister.Add(output);
                        }

                        return output;

                    });

            return writeStreamFactoryMock.Object;
        }

        private IWriteStreamToReadStreamConverter CreateMemoryWriteStreamToReadStreamConverter(Action<MemoryStream> streamConversionHandler)
        {
            var writeStreamToReadStreamConverterMock = new Mock<IWriteStreamToReadStreamConverter>();

            writeStreamToReadStreamConverterMock.Setup(writeStreamToReadStreamConverter => writeStreamToReadStreamConverter.ConvertWriteStreamToReadStream(
                It.Is<Stream>(stream => stream is MemoryStream))).Returns((Func<Stream, Stream>)(stream =>
                    {
                        stream.Position = 0;

                        if (streamConversionHandler != null)
                        {
                            streamConversionHandler((MemoryStream)stream);
                        }

                        return stream;
                    }));

            return writeStreamToReadStreamConverterMock.Object;
        }

        private IStreamDataSourceDisposerFactory CreateMemoryStreamDataSourceDisposerFactory(List<Stream> disposedStreamRegister)
        {
            var streamDataSourceDisposerFactoryMock = new Mock<IStreamDataSourceDisposerFactory>();

            streamDataSourceDisposerFactoryMock.Setup(streamDataSourceDisposerFactory => streamDataSourceDisposerFactory.CreateStreamDataSourceDisposer(
                It.Is<Stream>(stream => stream is MemoryStream))).Returns((Func<Stream, IDataSourceDisposer>)(stream =>
                    {
                        var dataSourceDisposerMock = new Mock<IDataSourceDisposer>();

                        dataSourceDisposerMock.Setup(dataSourceDisposer => dataSourceDisposer.DisposeDataSource())
                            .Callback(() => {
                                if (disposedStreamRegister != null)
                                {
                                    disposedStreamRegister.Add(stream);
                                }
                            });

                        return dataSourceDisposerMock.Object;
                    }));

            return streamDataSourceDisposerFactoryMock.Object;
        }

        private class ForwardableCurrentDateTimeSource : ICurrentDateTimeSource
        {

            private DateTime _currentDateTime;

            public ForwardableCurrentDateTimeSource(DateTime initial)
            {
                _currentDateTime = initial;
            }

            public void Forward(TimeSpan timeSpan)
            {
                _currentDateTime = _currentDateTime.Add(timeSpan);
            }

            public DateTime GetCurrentDateTime()
            {
                return _currentDateTime;
            }
        }

        private ICurrentDateTimeSource CreateFixedCurrentDateTimeSource()
        {
            var currentDateTimeSourceMock = new Mock<ICurrentDateTimeSource>();

            currentDateTimeSourceMock.Setup(
                currentDateTimeSource => currentDateTimeSource.GetCurrentDateTime())
                .Returns(DateTime.Now);

            return currentDateTimeSourceMock.Object;

        }

        [TestMethod]
        public void WhenReadStreamTemporarilyUnavailableReturnsFewerBytes()
        {

            var forwardableCurrentDateTimeSource = new ForwardableCurrentDateTimeSource(DateTime.Now);

            int readCount = 0;

            var writeStreamToReadStreamConverterMock = new Mock<IWriteStreamToReadStreamConverter>();

            writeStreamToReadStreamConverterMock.Setup(writeStreamToReadStreamConverter
                => writeStreamToReadStreamConverter.ConvertWriteStreamToReadStream(It.Is<Stream>(stream => stream is MemoryStream)))
                .Returns((Func<Stream, Stream>)(stream =>
                {
                    var memoryStream = stream as MemoryStream;

                    memoryStream.Position = 0;

                    var readStreamMock = new Mock<Stream>();

                    readStreamMock.Setup(readStream => readStream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                        .Returns((Func<byte[], int, int, int>)((buffer, offset, count) =>
                    {
                        return memoryStream.Read(buffer, offset, Math.Min(1, count));
                    }));

                    return readStreamMock.Object;

                }));

            var readStreamDataSourceDisposerFactoryMock = new Mock<IStreamDataSourceDisposerFactory>();

            var readStreamDataSourceDisposerMock = new Mock<IDataSourceDisposer>();

            readStreamDataSourceDisposerFactoryMock.Setup(readStreamDataSourceDisposerFactory =>
                readStreamDataSourceDisposerFactory.CreateStreamDataSourceDisposer(It.IsAny<Stream>()))
                .Returns(readStreamDataSourceDisposerMock.Object);

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(null),
                    writeStreamToReadStreamConverterMock.Object,
                    CreateMemoryStreamDataSourceDisposerFactory(null),
                    readStreamDataSourceDisposerFactoryMock.Object)
                .SetStreamLifecycleThresholds(
                    null,
                    null,
                    0)
                .SetCurrentDateTimeSource(
                    forwardableCurrentDateTimeSource).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

                var readResult = new byte[testData.Length];

                readCount = rollingMemory.ReadStream.Read(readResult, 0, readResult.Length);

                readCount += rollingMemory.ReadStream.Read(readResult, readCount, readResult.Length - readCount);

                Assert.IsTrue(readResult.Take(readCount).SequenceEqual(new byte[] { 1, 2 }));

            }

        }

        [TestMethod]
        public void WhenMaxAllowedWriteStreamBytesNotPassedWriteStreamRemainsUnconverted()
        {

            var forwardableCurrentDateTimeSource = new ForwardableCurrentDateTimeSource(DateTime.Now);

            long convertedWriteStreamBytes = 0;

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(null),
                    CreateMemoryWriteStreamToReadStreamConverter(stream => convertedWriteStreamBytes += stream.Length),
                    CreateMemoryStreamDataSourceDisposerFactory(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null))
                .SetStreamLifecycleThresholds(
                    4,
                    null,
                    null)
                .SetCurrentDateTimeSource(
                    forwardableCurrentDateTimeSource).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

                Assert.AreEqual(0, convertedWriteStreamBytes);

            }

        }

        [TestMethod]
        public void WhenMaxAllowedWriteStreamBytesPassedWriteStreamIsConvertedToReadStream()
        {

            var forwardableCurrentDateTimeSource = new ForwardableCurrentDateTimeSource(DateTime.Now);

            long convertedWriteStreamBytes = 0;

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(null),
                    CreateMemoryWriteStreamToReadStreamConverter(stream => convertedWriteStreamBytes += stream.Length),
                    CreateMemoryStreamDataSourceDisposerFactory(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null))
                .SetStreamLifecycleThresholds(
                    2,
                    null,
                    null)
                .SetCurrentDateTimeSource(
                    forwardableCurrentDateTimeSource).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

                Assert.IsTrue(convertedWriteStreamBytes >= 2);

            }

        }

        [TestMethod]
        public void WhenMaxUnavailableAgePassedDataIsAvailable()
        {

            var forwardableCurrentDateTimeSource = new ForwardableCurrentDateTimeSource(DateTime.Now);

            int readCount;

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(null),
                    CreateMemoryWriteStreamToReadStreamConverter(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null))
                .SetStreamLifecycleThresholds(
                    null,
                    TimeSpan.FromSeconds(1),
                    null)
                .SetCurrentDateTimeSource(
                    forwardableCurrentDateTimeSource).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

                var readResult = new byte[testData.Length];

                forwardableCurrentDateTimeSource.Forward(TimeSpan.FromSeconds(2));

                readCount = rollingMemory.ReadStream.Read(readResult, 0, readResult.Length);

            }

            Assert.AreEqual(4, readCount);

        }

        [TestMethod]
        public void WhenUnavailableLengthPassedDataIsAvailable()
        {

            var forwardableCurrentDateTimeSource = new ForwardableCurrentDateTimeSource(DateTime.Now);

            int readCount;

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(null),
                    CreateMemoryWriteStreamToReadStreamConverter(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null))
                .SetStreamLifecycleThresholds(
                    null,
                    null,
                    3)
                .SetCurrentDateTimeSource(
                    forwardableCurrentDateTimeSource).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

                var readResult = new byte[testData.Length];

                readCount = rollingMemory.ReadStream.Read(readResult, 0, readResult.Length);

            }

            Assert.IsTrue(readCount >= 1);

        }

        [TestMethod]
        public void WhenUnavailableLengthNotPassedDataButWriteStreamFlushCalledDataIsAvailable()
        {

            var forwardableCurrentDateTimeSource = new ForwardableCurrentDateTimeSource(DateTime.Now);

            int readCount;

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(null),
                    CreateMemoryWriteStreamToReadStreamConverter(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null))
                .SetStreamLifecycleThresholds(
                    null,
                    null,
                    4)
                .SetCurrentDateTimeSource(
                    forwardableCurrentDateTimeSource).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

                rollingMemory.WriteStream.Flush();

                var readResult = new byte[testData.Length];

                readCount = rollingMemory.ReadStream.Read(readResult, 0, readResult.Length);

            }

            Assert.AreEqual(4, readCount);

        }

        [TestMethod]
        public void WhenUnavailableLengthNotPassedDataIsUnavailable()
        {

            var forwardableCurrentDateTimeSource = new ForwardableCurrentDateTimeSource(DateTime.Now);

            int readCount;

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(null),
                    CreateMemoryWriteStreamToReadStreamConverter(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null))
                .SetStreamLifecycleThresholds(
                    null,
                    null,
                    4)
                .SetCurrentDateTimeSource(
                    forwardableCurrentDateTimeSource).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

                var readResult = new byte[testData.Length];

                readCount = rollingMemory.ReadStream.Read(readResult, 0, readResult.Length);

            }

            Assert.AreEqual(0, readCount);

        }

        [TestMethod]
        public void WhenMaxUnavailableAgeNotPassedDataIsUnavailable()
        {

            var forwardableCurrentDateTimeSource = new ForwardableCurrentDateTimeSource(DateTime.Now);

            int readCount;

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(null),
                    CreateMemoryWriteStreamToReadStreamConverter(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null))
                .SetStreamLifecycleThresholds(
                    null,
                    TimeSpan.FromSeconds(1),
                    null)
                .SetCurrentDateTimeSource(
                    forwardableCurrentDateTimeSource).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

                var readResult = new byte[testData.Length];

                readCount = rollingMemory.ReadStream.Read(readResult, 0, readResult.Length);

            }

            Assert.AreEqual(0, readCount);

        }


        [TestMethod]
        public void WhenDisposedAllStreamsDestroyed_AfterReading()
        {

            var createdStreamRegister = new List<Stream>();
            var disposedStreamRegister = new List<Stream>();

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(createdStreamRegister),
                    CreateMemoryWriteStreamToReadStreamConverter(null),
                    CreateMemoryStreamDataSourceDisposerFactory(disposedStreamRegister),
                    CreateMemoryStreamDataSourceDisposerFactory(disposedStreamRegister))
                .SetStreamLifecycleThresholds(
                    null,
                    null,
                    0)
                .SetCurrentDateTimeSource(
                   CreateFixedCurrentDateTimeSource()).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

                var readResult = new byte[testData.Length];

                rollingMemory.ReadStream.Read(readResult, 0, readResult.Length);

            }

            Assert.IsTrue(createdStreamRegister.Except(disposedStreamRegister).Count() == 0);

        }

        [TestMethod]
        public void WhenDisposedAllStreamsDestroyed_BeforeReading()
        {

            var createdStreamRegister = new List<Stream>();
            var disposedStreamRegister = new List<Stream>();

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(createdStreamRegister),
                    CreateMemoryWriteStreamToReadStreamConverter(null),
                    CreateMemoryStreamDataSourceDisposerFactory(disposedStreamRegister),
                    CreateMemoryStreamDataSourceDisposerFactory(disposedStreamRegister))
                .SetStreamLifecycleThresholds(
                    null,
                    null,
                    0)
                .SetCurrentDateTimeSource(
                   CreateFixedCurrentDateTimeSource()).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

            }

            Assert.IsTrue(createdStreamRegister.Except(disposedStreamRegister).Count() == 0);

        }

        [TestMethod]
        public void WhenZeroMaximumAllowedUnavailableBytesAllDataAvailable()
        {

            using (var rollingMemory = (new RollingMemoryBuilder())
                .SetupStreamLifecycleImplementation(
                    CreateMemoryWriteStreamFactory(null),
                    CreateMemoryWriteStreamToReadStreamConverter(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null),
                    CreateMemoryStreamDataSourceDisposerFactory(null))
                .SetStreamLifecycleThresholds(
                    null,
                    null,
                    0)
                .SetCurrentDateTimeSource(
                   CreateFixedCurrentDateTimeSource()).Build())
            {

                var testData = new byte[] { 1, 2, 3, 4 };

                rollingMemory.WriteStream.Write(testData, 0, testData.Length);

                var readResult = new byte[testData.Length];

                rollingMemory.ReadStream.Read(readResult, 0, readResult.Length);

                Assert.IsTrue(readResult.SequenceEqual(testData));

            }

        }

    }
}
