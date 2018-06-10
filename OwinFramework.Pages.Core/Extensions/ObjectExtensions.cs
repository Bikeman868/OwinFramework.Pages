using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Extensions
{
    /// <summary>
    /// Extension methods for Object
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Disposes of an object if it implements IDisposable, otherwise does nothing
        /// </summary>
        public static void Dispose(this object o)
        {
            var disposable = o as IDisposable;
            if (disposable != null) disposable.Dispose();
        }

        /// <summary>
        /// Returns IEnumerable{T} for a single instance
        /// </summary>
        public static IEnumerable<T> AsEnumerable<T>(this T instance)
        {
            return new List<T> { instance };
        }
    }
}
