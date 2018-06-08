using System;
using System.Linq;
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
        public Task[] TasksToWaitFor { get; set; }
        public bool IsComplete{ get; set; }

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

        public static IWriteResult WaitFor(Task task)
        {
            var writeResult = _factory.Create();
            writeResult.TasksToWaitFor = new []{ task};
            return writeResult;
        }

        public static IWriteResult ContinueAsync(Action action)
        {
            var writeResult = _factory.Create();
            writeResult.TasksToWaitFor = new[]{ new Task(action)};
            writeResult.TasksToWaitFor[0].Start();
            return writeResult;
        }

        public static IWriteResult Continue()
        {
            return null;
        }

        private WriteResult()
        {
        }

        public IWriteResult Add(IWriteResult prior)
        {
            if (prior == null)
                return this;

            if (prior.IsComplete) 
                IsComplete = true;

            if (prior.TasksToWaitFor != null)
            {
                if (TasksToWaitFor == null)
                {
                    TasksToWaitFor = prior.TasksToWaitFor;
                }
                else
                {
                    TasksToWaitFor = TasksToWaitFor.Concat(prior.TasksToWaitFor).ToArray();
                }
            }

            return this;
        }

        private new WriteResult Initialize(Action<IReusable> disposeAction)
        {
            TasksToWaitFor = null;
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
