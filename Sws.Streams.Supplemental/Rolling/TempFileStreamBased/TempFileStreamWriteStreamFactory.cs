using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Rolling;
using Sws.Streams.Supplemental.StreamImplementations;

namespace Sws.Streams.Supplemental.Rolling.TempFileStreamBased
{

    public class TempFileStreamWriteStreamFactory : IWriteStreamFactory
    {

        public Stream CreateWriteStream()
        {
            return new TempFileStream(FileMode.Create, FileAccess.Write);
        }

    }

}
