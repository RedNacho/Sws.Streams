using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Common
{
    public class CurrentDateTimeSource : ICurrentDateTimeSource
    {

        public DateTime GetCurrentDateTime()
        {
            return DateTime.UtcNow;
        }

    }
}
