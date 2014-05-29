using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.MemoryStreamBased
{

    public class MemoryStreamWriteStreamToReadStreamConverter : IWriteStreamToReadStreamConverter
    {
        public Stream ConvertWriteStreamToReadStream(Stream writeStream)
        {
            writeStream.Position = 0;

            return writeStream;
        }
    }

}
