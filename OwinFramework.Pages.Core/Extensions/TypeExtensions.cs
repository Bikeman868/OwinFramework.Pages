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
        /// Defines how to display the namespace of class names
        /// </summary>
        public enum NamespaceOption
        { 
            /// <summary>
            /// Do not include the namespace
            /// </summary>
            None, 

            /// <summary>
            /// Include the full namespace
            /// </summary>
            Full, 

            /// <summary>
            /// Include the last part of the namespace with ...
            /// </summary>
            Ending
        }

        /// <summary>
        /// Disposes of an object if it implements IDisposable, otherwise does nothing
        /// </summary>
        public static string DisplayName(this Type t, NamespaceOption namespaceOption = NamespaceOption.Full)
        {
            if (ReferenceEquals(t, null)) 
                return "[null]";

            var displayName = t.Name;

            if (t.IsValueType) return displayName;

            var ns = namespaceOption;
            if (t.Namespace.StartsWith("System") || t.IsNested || t.IsGenericType)
                ns = NamespaceOption.None;

            switch (ns)
            {
                case NamespaceOption.Ending:
                    var i = t.Namespace.LastIndexOf('.');
                    if (i < 0)
                        displayName = t.Namespace + '.' + displayName;
                    else
                        displayName = ".." + t.Namespace.Substring(i) + '.' + displayName;
                    break;
                case NamespaceOption.Full:
                    displayName = t.Namespace + '.' + displayName;
                    break;
            }

            if (t.IsGenericType)
            {
                displayName = displayName.Substring(0, displayName.IndexOf('`'));
                displayName += "<";
                displayName += string.Join(",", t.GetGenericArguments().Select(ga => ga.DisplayName(NamespaceOption.None)));
                displayName += ">";
            }

            if (t.IsNested)
            {
                displayName = t.DeclaringType.DisplayName(namespaceOption) + " { " + displayName + " }";
            }

            return displayName;
        }
    }
}
