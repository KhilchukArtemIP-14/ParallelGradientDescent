using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescent.TaskSchedulers
{
    public class LimitedParallelismScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool _threadIsBusy;
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();
        private readonly int _maxDegreeOfParallelism;
        private int _delegatesQueuedOrRunning = 0;
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        public LimitedParallelismScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new Exception("Maximum parallelism cannot be lower than 1");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        protected sealed override void QueueTask(Task task)
        {
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(WorkItem, null);
        }

        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!_threadIsBusy) return false;

            if (taskWasPreviouslyQueued)
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) 
            {
                return _tasks.Remove(task); 
            }
        }

        private void WorkItem(object state)
        {
            _threadIsBusy = true;
            try
            {
                while (true)
                {
                    Task item;
                    lock (_tasks)
                    {
                        if (_tasks.Count == 0)
                        {
                            Interlocked.Decrement(ref _delegatesQueuedOrRunning);
                            break;
                        }

                        item = _tasks.First.Value;
                        _tasks.RemoveFirst();
                    }

                    base.TryExecuteTask(item);
                }
            }
            finally 
            { 
                _threadIsBusy = false; 
            }
        }
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new Exception("Cannot get tasks");
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }
}
