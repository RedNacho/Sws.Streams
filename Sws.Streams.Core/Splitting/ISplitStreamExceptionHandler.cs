using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sws.Streams.Core.Splitting
{

    public interface ISplitStreamExceptionHandler
    {
        void HandleException(Stream targetStream, Exception exception);
    }

}
