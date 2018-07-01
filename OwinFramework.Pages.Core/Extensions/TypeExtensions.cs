using System;
using System.Linq;

namespace OwinFramework.Pages.Core.Extensions
{
    /// <summary>
    /// Extension methods for Type
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Disposes of an object if it implements IDisposable, otherwise does nothing
        /// </summary>
        public static string DisplayName(this Type t)
        {
            var displayName = t.Name;

            if (t.IsValueType) return displayName;

            if (!t.Namespace.StartsWith("System") && !t.IsNested)
                displayName = t.Namespace + '.' + displayName;

            if (t.IsGenericType)
            {
                displayName = displayName.Substring(0, displayName.IndexOf('`'));
                displayName += "<";
                displayName += string.Join(",", t.GetGenericArguments().Select(ga => ga.DisplayName()));
                displayName += ">";
            }

            if (t.IsNested)
            {
                displayName = t.DeclaringType.DisplayName() + " { " + displayName + " }";
            }

            return displayName;
        }
    }
}
