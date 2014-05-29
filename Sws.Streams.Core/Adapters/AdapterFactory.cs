using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sws.Streams.Core.Rolling;
using Sws.Streams.Core.Rewinding;

namespace Sws.Streams.Core.Adapters
{
    public static class AdapterFactory
    {

        public static RollingMemoryStream CreateRollingMemoryStream(IRollingMemory rollingMemory, bool disposeOfRollingMemoryOnDispose)
        {
            return new RollingMemoryStream(rollingMemory, disposeOfRollingMemoryOnDispose);
        }

        public static RewindableStream CreateRewindableStream(IRewindable rewindable, bool disposeOfRewindableOnDispose)
        {
            return new RewindableStream(rewindable, disposeOfRewindableOnDispose);
        }

    }
}
