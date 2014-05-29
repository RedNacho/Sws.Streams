using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling
{

    /// <summary>
    /// Creates a writable Stream.
    /// </summary>
    public interface IWriteStreamFactory
    {

        /// <summary>
        /// Creates a writable Stream.  The only requirement for this stream is that it is writable, the data store
        /// does not matter.
        /// </summary>
        /// <returns></returns>
        Stream CreateWriteStream();
    }

}
