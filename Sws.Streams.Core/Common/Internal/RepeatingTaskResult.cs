using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Streams.Core.Common.Internal
{
    internal class RepeatingTaskResult
    {
        private readonly bool _wasWorkDone;

        public bool WasWorkDone { get { return _wasWorkDone; } }

        private readonly bool _isWorkCompleted;

        public bool IsWorkCompleted { get { return _isWorkCompleted; } }

        public RepeatingTaskResult(bool wasWorkDone, bool isWorkCompleted)
        {
            _wasWorkDone = wasWorkDone;
            _isWorkCompleted = isWorkCompleted;
        }
    }
}
