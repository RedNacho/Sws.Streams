using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Adapters
{

    public abstract class StreamWrapperBase : Stream
    {

        private readonly Stream _readStream;

        private Stream ReadStream { get { return _readStream; } }

        private readonly Stream _writeStream;

        private Stream WriteStream { get { return _writeStream; } }

        private readonly Stream _seekStream;

        private Stream SeekStream { get { return _seekStream; } }

        private readonly IEnumerable<IDisposable> _disposables;

        private IEnumerable<IDisposable> Disposables { get { return _disposables; } }

        public StreamWrapperBase(Stream readStream, Stream writeStream, Stream seekStream, params IDisposable[] disposables)
        {
            if (readStream == null)
                throw new ArgumentNullException("readStream");

            if (writeStream == null)
                throw new ArgumentNullException("writeStream");

            if (seekStream == null)
                throw new ArgumentNullException("seekStream");

            if (disposables == null)
                disposables = Enumerable.Empty<IDisposable>().ToArray();

            _readStream = readStream;
            _writeStream = writeStream;
            _seekStream = seekStream;

            _disposables = disposables;

        }

        public override bool CanRead
        {
            get { return ReadStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return SeekStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return WriteStream.CanWrite; }
        }

        public override void Flush()
        {
            WriteStream.Flush();
        }

        public override long Length
        {
            get { return SeekStream.Length; }
        }

        public override long Position
        {
            get
            {
                return SeekStream.Position;
            }
            set
            {
                SeekStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return SeekStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            SeekStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                foreach (var disposable in Disposables)
                {
                    disposable.Dispose();
                }
            }

        }

    }

}
