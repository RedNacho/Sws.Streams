using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.MemoryStreamBased
{
    public class MemoryStreamWriteStreamFactory : IWriteStreamFactory
    {

        public Stream CreateWriteStream()
        {
            return new MemoryStream();
        }

    }
}
