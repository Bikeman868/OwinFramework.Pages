using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to make it part of a package (namespace)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PartOfAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that specifies the package that 
        /// this element belongs to
        /// </summary>
        /// <param name="packageName">The name of the package that this element belongs 
        /// to. Any JavaScript methods or css classes produced by this element
        /// will be in the namespace of this package</param>
        public PartOfAttribute(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// The name of the package
        /// </summary>
        public string PackageName { get; set; }
    }
}
