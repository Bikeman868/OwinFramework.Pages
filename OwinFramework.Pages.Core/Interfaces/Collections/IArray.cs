using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// Defines a generic array that can be re-used to avoid garbage collection
    /// </summary>
    public interface IArray<T> : IReusable, IEnumerable<T>
    {
        /// <summary>
        /// Extracts the array from within the instance
        /// </summary>
        T[] GetArray();

        /// <summary>
        /// Read/write elements of the array
        /// </summary>
        T this[long index] { get; set; }

        /// <summary>
        /// Returns the number of elements in the arry
        /// </summary>
        long Size { get; }
    }
}
