using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling
{

    /// <summary>
    /// Disposes of a data source
    /// </summary>
    public interface IDataSourceDisposer
    {

        /// <summary>
        /// Dispose the data source.
        /// </summary>
        void DisposeDataSource();
    }

}
