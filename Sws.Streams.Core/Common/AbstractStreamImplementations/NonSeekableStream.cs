using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Common.AbstractStreamImplementations
{

    public abstract class NonSeekableStream : Stream
    {
     
        public override sealed bool CanSeek
        {
            get { return false; }
        }

        public override sealed long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override sealed long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override sealed long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override sealed void SetLength(long value)
        {
            throw new NotSupportedException();
        }

    }

}
