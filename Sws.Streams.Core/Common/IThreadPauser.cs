using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Common
{
    public interface IThreadPauser
    {
        void Pause(TimeSpan interval);
    }
}
