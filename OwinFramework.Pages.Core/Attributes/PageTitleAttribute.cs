using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a page to set the page title
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PageTitleAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that specifies the package that 
        /// this element belongs to
        /// </summary>
        /// <param name="title">The name of the package that this element belongs 
        /// to. Any JavaScript methods or css classes produced by this element
        /// will be in the namespace of this package</param>
        public PageTitleAttribute(string title)
        {
            Title = title;
        }

        /// <summary>
        /// The title to put on the page
        /// </summary>
        public string Title { get; set; }
    }
}
