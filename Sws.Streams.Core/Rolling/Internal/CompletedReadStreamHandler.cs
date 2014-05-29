using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{
    internal class CompletedReadStreamHandler : ICompletedReadStreamHandler
    {

        private readonly IDataSourceDisposerRegister _dataSourceDisposerRegister;

        public IDataSourceDisposerRegister DataSourceDisposerRegister { get { return _dataSourceDisposerRegister; } }

        public CompletedReadStreamHandler(IDataSourceDisposerRegister dataSourceDisposerRegister)
        {
            _dataSourceDisposerRegister = dataSourceDisposerRegister;
        }

        public void HandleCompletedReadStream(Stream readStream)
        {
            readStream.Close();

            DataSourceDisposerRegister.Release(readStream);
        }

    }
}
