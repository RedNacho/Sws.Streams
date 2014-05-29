using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{
    internal class ReadStreamDefinition
    {

        public Stream Stream { get; private set; }
        public long Length { get; private set; }

        public ReadStreamDefinition(Stream stream, long length)
        {
            Stream = stream;
            Length = length;
        }

    }
}
