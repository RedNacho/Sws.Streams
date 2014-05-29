using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling
{

    /// <summary>
    /// Converts a writable Stream to a readable Stream.
    /// </summary>
    public interface IWriteStreamToReadStreamConverter
    {

        /// <summary>
        /// Converts a writable Stream to a sequentially readable Stream exposing the data which was written.  For
        /// example, if it is known that the Stream is a FileStream, the filename might be retrieved, then the
        /// writable stream closed, then a new Stream opened providing sequential read access.  If writeStream
        /// is fit-for-purpose, its position may be reset and then it may be returned, as it will not be reused
        /// for anything else.
        /// </summary>
        /// <param name="writeStream"></param>
        /// <returns></returns>
        Stream ConvertWriteStreamToReadStream(Stream writeStream);
    }

}
