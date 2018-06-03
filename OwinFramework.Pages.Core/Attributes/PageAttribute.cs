using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone page that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class PageAttribute: Attribute
    {
        /// <summary>
        /// Defines an optional name for this page so that is can be referenced 
        /// by name in other elements
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the name of the layout to use on this page
        /// </summary>
        public string Layout { get; set; }
    }
}
