using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Common.AbstractStreamImplementations
{
    public abstract class NonSeekableWriteOnlyStream : NonSeekableStream
    {

        public override sealed bool CanRead
        {
            get { return false; }
        }

        public override sealed int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override sealed IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override sealed int ReadByte()
        {
            throw new NotSupportedException();
        }

    }
}
