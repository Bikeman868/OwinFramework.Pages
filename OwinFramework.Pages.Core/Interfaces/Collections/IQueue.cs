using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// Defines the ability to store data with a first-in-first-out metaphor
    /// </summary>
    /// <typeparam name="T">The type of items to store in the queue</typeparam>
    public interface IQueue<T> : IDisposable
    {
        /// <summary>
        /// The maximum number of items to store in the queue, or 
        /// 0 to allow an unlimited number of items
        /// </summary>
        int Capacity { get; set; }

        /// <summary>
        /// Returns the number of items currently in the queue
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds a collection of data to the queue
        /// </summary>
        /// <param name="items">The list of items to add</param>
        /// <returns>True if all of the items were added to the queue and False
        /// if some or all of the items were not added</returns>
        bool Enqueue(IEnumerable<T> items);

        /// <summary>
        /// Adds a single item to the queue
        /// </summary>
        /// <param name="o">The data to add</param>
        /// <returns>True if the data was added and False if the queue was full</returns>
        bool Enqueue(T o);

        /// <summary>
        /// Waits for the queue to contain data and returns one item from the
        /// front of the queue.
        /// </summary>
        /// <param name="maximumWaitTime">How long to wait for the queue to contain data</param>
        /// <returns>The dequeued item or the default value for type T</returns>
        T Dequeue(TimeSpan maximumWaitTime);

        /// <summary>
        /// Returns one item from the front of the queue or default(T) if the queue is empty
        /// </summary>
        T DequeueOrDefault();

        /// <summary>
        /// Removes all of the items in the queue
        /// </summary>
        void Clear();

        /// <summary>
        /// Removes all the items in the queue, disposing of each one
        /// </summary>
        /// <param name="disposeAction">A lambda function that cleans up objects removed from the queue</param>
        void Clear(Action<T> disposeAction);
    }
}
