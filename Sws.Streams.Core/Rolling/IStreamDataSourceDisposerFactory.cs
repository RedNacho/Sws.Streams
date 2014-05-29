using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling
{

    /// <summary>
    /// Given a Stream, creates an object for disposing of the underlying data source.
    /// </summary>
    public interface IStreamDataSourceDisposerFactory
    {

        /// <summary>
        /// Creates the disposer object.  The Stream will be open at the point where this method is called;
        /// however, it will have been closed when the disposer is invoked.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        IDataSourceDisposer CreateStreamDataSourceDisposer(Stream stream);
    }
}
