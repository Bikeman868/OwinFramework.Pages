using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Collections;

namespace OwinFramework.Pages.Facilities.Runtime
{
    /// <summary>
    /// Use instances of this class to return information about the
    /// write operation. Instances can be constructed using one of
    /// the public static methods
    /// </summary>
    public class WriteResult: ReusableObject, IWriteResult
    {
        public bool IsComplete{ get; set; }

        private Task[] _tasksToWaitFor;
        private List<CancellationTokenSource> _cancellationSources;

        public static IWriteResult Create()
        {
            return _factory.Create();
        }

        public static IWriteResult ResponseComplete()
        {
            var writeResult = _factory.Create();
            writeResult.IsComplete = true;
            return writeResult;
        }

        public static IWriteResult WaitFor(Task task, CancellationTokenSource cancellation = null)
        {
            var writeResult = _factory.Create();
            
            if (task != null)
                writeResult._tasksToWaitFor = new []{ task };

            if (cancellation != null)
                writeResult._cancellationSources = new List<CancellationTokenSource> { cancellation };

            return writeResult;
        }

        public static IWriteResult WaitFor(params Task[] tasks)
        {
            if (tasks == null || tasks.Length == 0)
                return null;

            var writeResult = _factory.Create();
            writeResult._tasksToWaitFor = tasks;
            return writeResult;
        }

        public static IWriteResult WaitFor(
            IEnumerable<Task> tasks, 
            IEnumerable<CancellationTokenSource> cancellations = null)
        {
            var writeResult = _factory.Create();

            if (tasks != null)
                writeResult._tasksToWaitFor = tasks.ToArray();

            if (cancellations != null)
                writeResult._cancellationSources = cancellations.ToList();

            return writeResult;
        }

        public static IWriteResult ContinueAsync(Action action)
        {
            var writeResult = _factory.Create();

            var cancellation = new CancellationTokenSource();
            var task = new Task(action, cancellation.Token);
            task.Start();

            writeResult._tasksToWaitFor = new[]{ task };
            writeResult._cancellationSources = new List<CancellationTokenSource> { cancellation };

            return writeResult;
        }

        public static IWriteResult Continue()
        {
            return null;
        }

        private WriteResult()
        {
        }

        public IWriteResult Add(IWriteResult priorWriteResult)
        {
            var prior = priorWriteResult as WriteResult;

            if (prior == null)
                return this;

            if (prior.IsComplete) 
                IsComplete = true;

            if (prior._tasksToWaitFor != null && prior._tasksToWaitFor.Length > 0)
            {
                if (_tasksToWaitFor == null)
                {
                    _tasksToWaitFor = prior._tasksToWaitFor;
                }
                else
                {
                    _tasksToWaitFor = _tasksToWaitFor.Concat(prior._tasksToWaitFor).ToArray();
                }
            }

            return this;
        }

        public void Wait(bool cancel = false)
        {
            if (_cancellationSources != null)
            {
                foreach (var cancellation in _cancellationSources)
                {
                    cancellation.Cancel();
                }
            }

            if (_tasksToWaitFor != null && _tasksToWaitFor.Length > 0)
            {
                try
                {
                    Task.WaitAll(_tasksToWaitFor);
                }
                catch (AggregateException e)
                {
                    // This happens when the tasks are cancelled
                }
                finally
                {
                    if (_cancellationSources != null)
                    {
                        foreach (var cancellation in _cancellationSources)
                        {
                            cancellation.Dispose();
                        }
                    }
                }
            }
        }

        private new WriteResult Initialize(Action<IReusable> disposeAction)
        {
            _tasksToWaitFor = null;
            _cancellationSources = null;
            IsComplete = false;

            base.Initialize(disposeAction);

            return this;
        }

        private static readonly Factory _factory = new Factory(new QueueFactory());

        private class Factory: ReusableObjectFactory
        {
            public Factory(IQueueFactory queueFactory)
                : base(queueFactory)
            {
                Initialize(500);
            }

            public WriteResult Create()
            {
                var writeResult = Queue.DequeueOrDefault() as WriteResult ?? new WriteResult();
                return writeResult.Initialize(DisposeAction);
            }
        }
    }
}
