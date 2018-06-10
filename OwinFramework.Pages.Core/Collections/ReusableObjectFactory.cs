using System;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Collections
{
    /// <summary>
    /// Base class for factories that pool and re-use objects. Pooling and re-using object
    /// avoids garbage collection.
    /// </summary>
    public class ReusableObjectFactory: Disposable
    {
        /// <summary>
        /// When you construct reusable objects you should pass this
        /// delegate to them. When they are disposed they should call
        /// this to add themselves back into the pool ready for reuse
        /// </summary>
        protected readonly Action<IReusable> DisposeAction;

        /// <summary>
        /// Contains a pool of instances that are not being used and
        /// are ready to be reused
        /// </summary>
        protected IQueue<IReusable> Queue;

        private readonly IQueueFactory _queueFactory;

        /// <summary>
        /// Base constructor must be called from your derrived class
        /// </summary>
        /// <param name="queueFactory"></param>
        public ReusableObjectFactory(IQueueFactory queueFactory)
        {
            _queueFactory = queueFactory;
            DisposeAction = reusable =>
                {
                    if (reusable.IsReusable) Queue.Enqueue(reusable);
                };
        }

        /// <summary>
        /// Creates a pool for reusable instances
        /// </summary>
        /// <param name="capacity">The maximum number of instances to keep in the pool</param>
        protected ReusableObjectFactory Initialize(int capacity)
        {
            Queue = _queueFactory.Create<IReusable>(capacity);
            return this;
        }

        /// <summary>
        /// When this instance is disposed it disposes of 
        /// the pool of reusable instances
        /// </summary>
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
