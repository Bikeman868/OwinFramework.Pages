using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Provides an html description of the endpoint
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DescriptionAttribute: Attribute
    {
        /// <summary>
        /// A fragment of HTML that describes this element
        /// </summary>
        public string Html { get; set; }
    }
}
