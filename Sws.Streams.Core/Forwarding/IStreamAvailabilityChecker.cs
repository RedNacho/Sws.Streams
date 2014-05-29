using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Forwarding
{

    public interface IStreamAvailabilityChecker
    {

        /// <summary>
        /// Indicates the maximum amount of data available to read from the Stream, or null if unknown.
        /// </summary>
        int? DataAvailable { get; }
    }

}
