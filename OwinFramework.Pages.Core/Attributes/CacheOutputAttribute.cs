using System;
using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a page or service to indicate that the putput
    /// can be cached under certain conditions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CacheOutputAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that specifies how the output from this runable should be cached
        /// </summary>
        /// <param name="cacheCategory">Define rules in the Output Cache middleware that use these category names</param>
        /// <param name="cachePriority">Defines how expensive this page it to produce and therefore how worthwhile it is to cache it</param>
        public CacheOutputAttribute(string cacheCategory, CachePriority cachePriority)
        {
            CacheCategory = cacheCategory;
            CachePriority = cachePriority;
        }

        /// <summary>
        /// The category name that is used in the Output Cache rules
        /// </summary>
        public string CacheCategory { get; set; }

        /// <summary>
        /// The css style definition
        /// </summary>
        public CachePriority CachePriority { get; set; }
    }
}
