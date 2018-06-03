using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to each package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class PackageAttribute: Attribute
    {
        /// <summary>
        /// Defines an optional name for this package so that is can be referenced 
        /// by name in other elements
        /// </summary>
        public string Name { get; set; }
    }
}
