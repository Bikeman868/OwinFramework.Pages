using System;
using System.Collections;
using System.Collections.Generic;

namespace OwinFramework.Pages.Facilities.Collections
{
    /// <summary>
    /// Extension methods for Object
    /// </summary>
    public static class ObjectExtensions
    {
        public static void Dispose(this object o)
        {
            var disposable = o as IDisposable;
            if (disposable != null) disposable.Dispose();
        }

        public static IEnumerable<T> AsEnumerable<T>(this T instance)
        {
            var result = new List<T>();
            result.Add(instance);
            return result;
        }

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable enumerable)
        {
            var result = new List<T>();
            foreach (T instance in enumerable)
                result.Add(instance);
            return result;
        }
    }
}
