using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{
    internal interface IDataSourceDisposerRegister
    {
        void Register(Stream stream, IDataSourceDisposer dataSourceDisposer);
        void Swap(Stream original, Stream replacement, IDataSourceDisposer replacementDataSourceDisposer);
        void Release(Stream stream);
    }
}
