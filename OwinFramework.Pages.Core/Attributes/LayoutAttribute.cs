using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone layout that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class LayoutAttribute: Attribute
    {
        /// <summary>
        /// Defines an optional name for this layout so that is can be referenced 
        /// by name in other elements
        /// </summary>
        public string Name { get; set; }
    }
}
