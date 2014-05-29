using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling
{

    /// <summary>
    /// Allows read and write access to a rolling memory store.
    /// </summary>
    public interface IRollingMemory : IDisposable
    {
        /// <summary>
        /// Stream allowing sequential write access to the rolling memory store.
        /// </summary>
        Stream WriteStream { get; }

        /// <summary>
        /// Stream allowing sequential read access to the rolling memory store.
        /// </summary>
        Stream ReadStream { get; }
    }

}
