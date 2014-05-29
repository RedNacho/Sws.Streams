using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Rolling;
using Sws.Streams.Supplemental.Common.Internal;
using Sws.Streams.Supplemental.StreamImplementations;

namespace Sws.Streams.Supplemental.Rolling.TempFileStreamBased
{
    public class TempFileStreamWriteStreamToReadStreamConverter : IWriteStreamToReadStreamConverter
    {

        public Stream ConvertWriteStreamToReadStream(Stream writeStream)
        {
            if (writeStream == null)
                throw new ArgumentNullException("writeStream");

            TempFileStream tempFileWriteStream = writeStream as TempFileStream;

            if (tempFileWriteStream == null)
                throw new ArgumentException(string.Format(ExceptionMessages.ValueMustBeTempFileStreamFormat, "writeStream"), "writeStream");

            return tempFileWriteStream.CloseAndReopen(FileMode.Open, FileAccess.Read);
        }

    }
}
