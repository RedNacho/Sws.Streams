using System.Collections.Generic;
using System.Linq;

namespace Sws.Streams.Core.Rolling.Internal
{
    internal class CompositeRollingMemoryStateMonitor : IRollingMemoryStateMonitor
    {
        private readonly IEnumerable<IRollingMemoryStateMonitor> _components;

        public CompositeRollingMemoryStateMonitor(params IRollingMemoryStateMonitor[] components)
        {
            _components = components.Where(c => c != null).ToList();
        }

        public void WritePositionAdvanced(long delta)
        {
            foreach (IRollingMemoryStateMonitor component in _components)
            {
                component.WritePositionAdvanced(delta);
            }
        }

        public void ReadPositionAdvanced(long delta)
        {
            foreach (IRollingMemoryStateMonitor component in _components)
            {
                component.ReadPositionAdvanced(delta);
            }
        }
    }
}
