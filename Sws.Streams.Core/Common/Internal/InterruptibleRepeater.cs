using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sws.Streams.Core.Common.Internal
{

    internal class InterruptibleRepeater : IInterruptibleRepeater
    {
        
        private readonly TimeSpan? _pollInterval;

        public TimeSpan? PollInterval { get { return _pollInterval; } }

        private readonly IExceptionHandler _exceptionHandler;

        public IExceptionHandler ExceptionHandler { get { return _exceptionHandler; } }

        private readonly IThreadPauser _threadPauser;

        public IThreadPauser ThreadPauser { get { return _threadPauser; } }

        private bool StopRequested { get; set; }

        private Thread RunningThread { get; set; }

        private readonly object _startStopSyncObject = new object();

        private object StartStopSyncObject { get { return _startStopSyncObject; } }

        private Barrier TerminationBarrier { get; set; }

        public InterruptibleRepeater(
            TimeSpan? pollInterval,
            IThreadPauser threadPauser,
            IExceptionHandler exceptionHandler)
        {
        
            if (threadPauser == null)
                throw new ArgumentNullException("threadPauser");

            _pollInterval = pollInterval;
            _threadPauser = threadPauser;
            _exceptionHandler = exceptionHandler;
            
        }

        public bool IsRunning
        {
            get
            {
                lock (StartStopSyncObject)
                {
                    return (RunningThread != null);
                }
            }
        }

        private void EnterRunningState()
        {
            lock (StartStopSyncObject)
            {
                if (RunningThread != null)
                    throw new InvalidOperationException(ExceptionMessages.StreamForwarderAlreadyRunning);

                RunningThread = Thread.CurrentThread;

                StopRequested = false;

                TerminationBarrier = new Barrier(1);
            }
        }

        private void EnterStoppedState()
        {
            lock (StartStopSyncObject)
            {
                RunningThread = null;

                StopRequested = false;

                TerminationBarrier.SignalAndWait();

                TerminationBarrier.Dispose();

                TerminationBarrier = null;
            }
        }

        public void Run(IRepeatingTask repeatingTask)
        {

            if (repeatingTask == null)
                throw new ArgumentNullException("repeatingTask");

            EnterRunningState();

            bool stopRequested = false;

            try
            {

                while (!stopRequested)
                {

                    var repeatingTaskResult = DoTask(repeatingTask);

                    if (repeatingTaskResult.IsWorkCompleted)
                    {
                        Stop();
                    }

                    stopRequested = CheckStopRequested();

                    if ((!stopRequested) && (!repeatingTaskResult.WasWorkDone) && PollInterval.HasValue)
                    {
                        ThreadPauser.Pause(PollInterval.Value);

                        stopRequested = CheckStopRequested();
                    }

                }
            }
            finally
            {
                EnterStoppedState();
            }

        }

        private bool CheckStopRequested()
        {
            lock (StartStopSyncObject)
            {
                return this.StopRequested;
            }
        }

        private RepeatingTaskResult DoTask(IRepeatingTask repeatingTask)
        {
            RepeatingTaskResult output = null;

            try
            {
                output = repeatingTask.DoTask();
            }
            catch (Exception exception)
            {
                if (ExceptionHandler != null)
                {
                    ExceptionHandler.HandleException(exception);
                }
                else
                {
                    throw;
                }
            }

            if (output == null)
            {
                output = new RepeatingTaskResult(false, false);
            }

            return output;

        }

        public void Stop()
        {
            Barrier terminationBarrier = null;

            lock (StartStopSyncObject)
            {
                StopRequested = true;

                if (TerminationBarrier != null && RunningThread != Thread.CurrentThread)
                {
                    terminationBarrier = TerminationBarrier;
                    terminationBarrier.AddParticipant();
                }

            }

            if (terminationBarrier != null)
            {
                terminationBarrier.SignalAndWait();
            }

        }

    }

}
