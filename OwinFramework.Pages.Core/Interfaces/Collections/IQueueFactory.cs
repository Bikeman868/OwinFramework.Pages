namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// Defines the ability to return implementations of IQueue
    /// </summary>
    public interface IQueueFactory
    {
        /// <summary>
        /// Creates a new thread-safe queue that does not thrash the garbage collector
        /// </summary>
        /// <typeparam name="T">The type if items in the queue</typeparam>
        /// <param name="capacity">The initial capacity of the queue. Expands as needed</param>
        IQueue<T> Create<T>(int capacity = 0);
    }
}
