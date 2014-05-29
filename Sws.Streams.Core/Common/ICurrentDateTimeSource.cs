using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Common
{

    /// <summary>
    /// Retrieves the current DateTime from the system.  CurrentDateTimeSource implementation should always be used
    /// except when unit testing.
    /// </summary>
    public interface ICurrentDateTimeSource
    {

        /// <summary>
        /// Retrieves the current DateTime (CurrentDateTimeSource implementation returns DateTime.UtcNow).
        /// </summary>
        /// <returns></returns>
        DateTime GetCurrentDateTime();
    }
}
