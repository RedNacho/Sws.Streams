using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Common
{

    public interface IExceptionHandler
    {
        void HandleException(Exception exception);
    }

}
