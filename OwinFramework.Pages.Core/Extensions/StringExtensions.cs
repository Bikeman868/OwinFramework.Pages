using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Extensions
{
    /// <summary>
    /// Extension methods for Object
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a new string with the first letter capitalized.
        /// Returns the same string if the first letter is already capitalized
        /// </summary>
        public static string InitialCaps(this string s)
        {
            if (string.IsNullOrEmpty(s) || char.IsUpper(s[0])) return s;
            return new string(char.ToUpper(s[0]), 1) + s.Substring(1);
        }
    }
}
