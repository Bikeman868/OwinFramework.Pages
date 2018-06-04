using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone region that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsRegionAttribute: Attribute
    {
        /// <summary>
        /// Constructe a new attribute that indicates the class is a region
        /// </summary>
        /// <param name="name">The name of this region. Must be unique within a package</param>
        public IsRegionAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Defines an optional name for this region so that is can be referenced 
        /// by name in other elements
        /// </summary>
        public string Name { get; set; }
    }
}
