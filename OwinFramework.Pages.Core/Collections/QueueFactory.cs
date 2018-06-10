using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Core.Collections
{
    /// <summary>
    /// A factory for creating queues
    /// </summary>
    public class QueueFactory: IQueueFactory
    {
        /// <summary>
        /// Creates and initializes a new thread safe queue
        /// </summary>
        public IQueue<T> Create<T>(int capacity = 0)
        {
            return new ThreadSafeQueue<T>().Initialize(capacity);
        }
    }
}
