using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.MemoryStreamBased
{
    public class MemoryStreamStreamDataSourceDisposerFactory : IStreamDataSourceDisposerFactory
    {

        public IDataSourceDisposer CreateStreamDataSourceDisposer(Stream stream)
        {
            return new MemoryStreamDataSourceDisposer();
        }

        private class MemoryStreamDataSourceDisposer : IDataSourceDisposer
        {

            public void DisposeDataSource()
            {
            }

        }

    }
}
