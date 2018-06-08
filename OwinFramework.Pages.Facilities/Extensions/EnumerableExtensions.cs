using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OwinFramework.Pages.Facilities.Extensions
{
    /// <summary>
    /// Extension methods for IEnumerable
    /// </summary>
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable enumerable)
        {
            return enumerable.Cast<T>().ToList();
        }
    }
}
