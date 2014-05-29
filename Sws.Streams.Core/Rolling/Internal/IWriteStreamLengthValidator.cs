using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal interface IWriteStreamLengthValidator
    {
        long RequestAvailableLength(long idealLength);
    }

}
