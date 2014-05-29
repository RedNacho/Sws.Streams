using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Rewinding
{

    /// <summary>
    /// Provides Stream-based access to an underlying cache.
    /// </summary>

    public interface ICacheAccessor
    {
        /// <summary>
        /// Carries out an action on a Stream which allows sequential write access to the end of the cache.
        /// This is used for append operations.
        /// </summary>
        /// <param name="action">The action to carry out</param>
        void ActOnCacheWriteStream(Action<Stream> action);

        /// <summary>
        /// Carries out an action on a Stream which allows sequential read access from a given position
        /// before the end of the cache
        /// </summary>
        /// <param name="action">The action to carry out</param>
        /// <param name="distanceFromEnd">The distance from the end of the cache that the Stream's position
        /// must be when action begins, e.g. a value of 5 indicates that a Read operation would read the
        /// last five bytes in the cache.</param>
        void ActOnCacheReadStream(Action<Stream> action, long distanceFromEnd);

        /// <summary>
        /// Truncates the cache, leaving the specified number of bytes available at the end.  A tailLength
        /// of 0 clears the cache completely.  An attempt to read from the cache before this position
        /// may result in unpredictable behaviour.
        /// </summary>
        /// <param name="tailLength"></param>
        void TruncateCache(long tailLength);

        /// <summary>
        /// Destroys the cache.
        /// </summary>
        void DestroyCache();

    }

}
