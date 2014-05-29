using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sws.Streams.Core.Rolling;

namespace Sws.Streams.Core.Adapters
{
    public class RollingMemoryStream : StreamWrapperBase
    {

        private readonly IRollingMemory _rollingMemory;

        public IRollingMemory RollingMemory { get { return _rollingMemory; } }

        public RollingMemoryStream(IRollingMemory rollingMemory, bool disposeOfRollingMemoryOnDispose)
            : base(rollingMemory.ReadStream, rollingMemory.WriteStream, rollingMemory.ReadStream, GetDisposables(rollingMemory, disposeOfRollingMemoryOnDispose))
        {
            _rollingMemory = rollingMemory;
        }

        private static IDisposable[] GetDisposables(IRollingMemory rollingMemory, bool disposeOfRollingMemoryOnDispose)
        {
            if (disposeOfRollingMemoryOnDispose)
            {
                return new IDisposable[] { rollingMemory };
            }

            return null;
        }


    }
}
