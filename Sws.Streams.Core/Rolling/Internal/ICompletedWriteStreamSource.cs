using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Rolling.Internal
{
    internal interface ICompletedWriteStreamSource
    {

        /// <summary>
        /// If there are outstanding completed write Streams available, pushes them to the read Stream queue.
        /// </summary>
        void FlushAllCompletedWriteStreamsToReadStreamQueue();
    }
}
