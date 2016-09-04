using System.Threading;

namespace Sws.Streams.Core.Rolling.Internal
{
    internal class LagCountingRollingMemoryStateMonitor : IRollingMemoryStateMonitor
    {
        private long _lag = 0;

        private readonly object _lagSyncObject = new object();

        public void WritePositionAdvanced(long delta)
        {
            ChangeLag(delta);
        }

        public void ReadPositionAdvanced(long delta)
        {
            ChangeLag(-delta);
        }

        public void AcquireLagChangeLock()
        {
            Monitor.Enter(_lagSyncObject);
        }

        public void ReleaseLagChangeLock()
        {
            Monitor.Exit(_lagSyncObject);
        }

        public void WaitForLagChange()
        {
            Monitor.Wait(_lagSyncObject);
        }

        private void SignalLagChange()
        {
            Monitor.PulseAll(_lagSyncObject);
        }

        public long Lag
        {
            get { return _lag; }
        }

        private void ChangeLag(long delta)
        {
            AcquireLagChangeLock();

            try
            {
                _lag += delta;
                SignalLagChange();
            }
            finally
            {
                ReleaseLagChangeLock();
            }   
        }
    }
}
