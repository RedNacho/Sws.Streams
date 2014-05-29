using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rolling.Internal
{

    internal class ReadStreamQueue : IReadStreamQueueTarget, IReadStreamQueueSource
    {
        
        private readonly Queue<ReadStreamDefinition> _readStreams = new Queue<ReadStreamDefinition>();

        private Queue<ReadStreamDefinition> ReadStreams { get { return _readStreams; } }

        private readonly object _readStreamsSyncObject = new object();

        private object ReadStreamsSyncObject { get { return _readStreamsSyncObject; } }

        public void EnqueueStream(ReadStreamDefinition readStreamDefinition)
        {
            lock (ReadStreamsSyncObject)
            {
                ReadStreams.Enqueue(readStreamDefinition);
            }
        }

        public ReadStreamDefinition DequeueStream()
        {
            ReadStreamDefinition output = null;

            lock (ReadStreamsSyncObject)
            {
                if (ReadStreams.Count > 0)
                {
                    output = ReadStreams.Dequeue();
                }
            }

            return output;
        }

    }
}
