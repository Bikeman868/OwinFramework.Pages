using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Facilities.Collections
{
    internal class QueueFactory: IQueueFactory
    {
        public IQueue<T> Create<T>(int capacity = 0)
        {
            return new ThreadSafeQueue<T>().Initialize(capacity);
        }
    }
}
