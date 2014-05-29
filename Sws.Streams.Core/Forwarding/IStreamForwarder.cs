using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Forwarding
{

    /// <summary>
    /// Continually reads from one Stream and writes to another.
    /// </summary>
    public interface IStreamForwarder
    {

        /// <summary>
        /// Indicates whether the forwarder is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Runs the forwarder.  This will block until all data from the source Stream has been forwarded or until Stop is called.
        /// </summary>
        void Run();

        /// <summary>
        /// Stops the forwarder if it is running.  If called on a thread other than the thread that the forwarder is running on,
        /// blocks until it has stopped.
        /// </summary>
        void Stop();
    }

}
