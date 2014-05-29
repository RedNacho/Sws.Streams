using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Common.AbstractStreamImplementations
{
    public abstract class NonSeekableReadOnlyStream : NonSeekableStream
    {

        public override sealed bool CanWrite
        {
            get { return false; }
        }

        public override sealed void Flush()
        {
        }

        public override sealed void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override sealed IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override sealed void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

    }
}
