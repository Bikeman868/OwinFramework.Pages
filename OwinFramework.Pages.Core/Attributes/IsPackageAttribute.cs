using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to each package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsPackageAttribute: Attribute
    {
        /// <summary>
        /// Constructs an attribute that identifies a class as a package
        /// </summary>
        /// <param name="name">The name of the package. Must be unique accross the 
        /// whole website, and must be valid as part of a css class name or
        /// JavaScript function name</param>
        public IsPackageAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Defines an optional name for this package so that is can be referenced 
        /// by name in other elements
        /// </summary>
        public string Name { get; set; }
    }
}
