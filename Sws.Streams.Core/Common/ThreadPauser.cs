using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sws.Streams.Core.Common
{
    public class ThreadPauser : IThreadPauser
    {

        public void Pause(TimeSpan interval)
        {
            Thread.Sleep(interval);
        }

    }
}
