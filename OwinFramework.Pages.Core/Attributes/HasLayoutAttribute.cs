using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a layout to define its regions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class HasLayoutAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defined the component
        /// to place in a region of a page
        /// </summary>
        /// <param name="layoutName">The name of layout to use on this page or in this region</param>
        public HasLayoutAttribute(string layoutName)
        {
            LayoutName = layoutName;
        }

        /// <summary>
        /// The name of the layout
        /// </summary>
        public string LayoutName { get; set; }
    }
}
