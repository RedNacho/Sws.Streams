using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal interface IReadStreamQueueTarget
    {

        /// <summary>
        /// Queues a read Stream to be read later.
        /// </summary>
        /// <param name="stream"></param>
        void EnqueueStream(ReadStreamDefinition readStreamDefinition);
    }

}
