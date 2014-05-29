using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal interface IReadStreamQueueSource
    {

        /// <summary>
        /// If a read Stream is available, dequeues it.
        /// </summary>
        /// <returns></returns>
        ReadStreamDefinition DequeueStream();
    }

}
