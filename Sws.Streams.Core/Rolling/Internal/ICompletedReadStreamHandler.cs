using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal interface ICompletedReadStreamHandler
    {

        /// <summary>
        /// Invoked when a read Stream is no longer required.
        /// </summary>
        /// <param name="readStream"></param>
        void HandleCompletedReadStream(Stream readStream);
    }

}
