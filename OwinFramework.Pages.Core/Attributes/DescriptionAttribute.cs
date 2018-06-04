using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Provides an html description of the endpoint
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DescriptionAttribute: Attribute
    {
        /// <summary>
        /// Constructs an attribute that provides a description of this element
        /// </summary>
        /// <param name="html">A fragment of html</param>
        public DescriptionAttribute(string html)
        {
            Html = html;
        }

        /// <summary>
        /// A fragment of HTML that describes this element
        /// </summary>
        public string Html { get; set; }
    }
}
