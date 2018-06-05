using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a page or region to fill the region with the specified layout
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class HasLayoutAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines the layout
        /// to place in a region or a page
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
