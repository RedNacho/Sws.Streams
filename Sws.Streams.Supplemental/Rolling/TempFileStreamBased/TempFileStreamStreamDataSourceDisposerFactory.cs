using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sws.Streams.Core.Rolling;

namespace Sws.Streams.Supplemental.Rolling.TempFileStreamBased
{
    public class TempFileStreamStreamDataSourceDisposerFactory : IStreamDataSourceDisposerFactory
    {

        public IDataSourceDisposer CreateStreamDataSourceDisposer(Stream stream)
        {
            return new DataSourceDisposer();
        }

        private class DataSourceDisposer : IDataSourceDisposer
        {

            public void DisposeDataSource()
            {
            }

        }

    }
}
