using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Rolling.Internal
{
    internal interface IReadStreamQueueRefresher
    {
        void RefreshReadStreamQueue();
    }
}
