using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Common.Internal
{
    internal interface IRepeatingTask
    {
        RepeatingTaskResult DoTask();
    }
}
