using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Forwarding
{

    public interface IStreamCompletionChecker
    {
        bool StreamCompleted { get; }
    }

}
