using System;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Collections
{
    /// <summary>
    /// Base class for factories that pool and re-use objects. Pooling and re-using object
    /// avoids garbage collection.
    /// </summary>
    public class ReusableObjectFactory: Disposable
    {
        protected readonly Action<IReusable> DisposeAction;
        protected IQueue<IReusable> Queue;

        private readonly IQueueFactory _queueFactory;

        public ReusableObjectFactory(IQueueFactory queueFactory)
        {
            _queueFactory = queueFactory;
            DisposeAction = reusable =>
                {
                    if (reusable.IsReusable) Queue.Enqueue(reusable);
                };
        }

        protected ReusableObjectFactory Initialize(int capacity)
        {
            Queue = _queueFactory.Create<IReusable>(capacity);
            return this;
        }

        protected override void Dispose(bool destructor)
        {
            if (!destructor)
            {
                Queue.Dispose();
            }
            base.Dispose(destructor);
        }
    }
}
