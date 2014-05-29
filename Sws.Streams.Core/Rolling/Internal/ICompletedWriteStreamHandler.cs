using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal interface ICompletedWriteStreamHandler
    {

        /// <summary>
        /// Invoked when a write Stream is longer being written to.
        /// </summary>
        /// <param name="writeStream"></param>
        void HandleCompletedWriteStream(Stream writeStream, long bytesWritten);
    }

}
