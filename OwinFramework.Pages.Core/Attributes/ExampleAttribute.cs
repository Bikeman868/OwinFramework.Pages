using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Provides an example of how to call this page or service
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ExampleAttribute: Attribute
    {
        /// <summary>
        /// Constructs an attribute that provides an example of how to use the endpoint
        /// </summary>
        /// <param name="html">A fragment of html</param>
        public ExampleAttribute(string html)
        {
            Html = html;
        }

        /// <summary>
        /// A fragment of HTML that describes an example of hoe to use this element
        /// </summary>
        public string Html { get; set; }
    }
}
