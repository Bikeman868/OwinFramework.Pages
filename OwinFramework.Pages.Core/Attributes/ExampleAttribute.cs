using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone service that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ExampleAttribute: Attribute
    {
        /// <summary>
        /// A fragment of HTML that describes an example of hoe to use this element
        /// </summary>
        public string Html { get; set; }
    }
}
