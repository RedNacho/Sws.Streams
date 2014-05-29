using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal class DataSourceDisposerRegister : IDataSourceDisposerRegister
    {

        private readonly IDictionary<Stream, IDataSourceDisposer> _dataSourceDisposers = new Dictionary<Stream, IDataSourceDisposer>();

        private IDictionary<Stream, IDataSourceDisposer> DataSourceDisposers { get { return _dataSourceDisposers; } }

        private readonly object _dataSourceDisposersSyncObject = new object();

        private object DataSourceDisposersSyncObject { get { return _dataSourceDisposersSyncObject; } }

        public void Register(Stream stream, IDataSourceDisposer dataSourceDisposer)
        {
            lock (DataSourceDisposersSyncObject)
            {
                DataSourceDisposers[stream] = dataSourceDisposer;
            }
        }

        public void Swap(Stream original, Stream replacement, IDataSourceDisposer replacementDataSourceDisposer)
        {
            lock (DataSourceDisposersSyncObject)
            {
                if (DataSourceDisposers.ContainsKey(original))
                {
                    DataSourceDisposers.Remove(original);
                }

                DataSourceDisposers[replacement] = replacementDataSourceDisposer;
            }
        }

        public void Release(Stream stream)
        {
            lock (DataSourceDisposersSyncObject)
            {
                if (DataSourceDisposers.ContainsKey(stream))
                {
                    var dataSourceDisposer = DataSourceDisposers[stream];

                    dataSourceDisposer.DisposeDataSource();

                    DataSourceDisposers.Remove(stream);

                }
            }
        }

    }

}
