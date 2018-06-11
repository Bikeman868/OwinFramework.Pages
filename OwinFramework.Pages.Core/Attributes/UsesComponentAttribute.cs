using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a region to fill the region with the specified component
    /// Attach one or more of these attributes to a page to add non-visual components to the page
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class UsesComponentAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines the layout
        /// to place in a region or a page
        /// </summary>
        /// <param name="componentName">The name of layout to use on this page or in this region</param>
        public UsesComponentAttribute(string componentName)
        {
            ComponentName = componentName;
        }

        /// <summary>
        /// The name of the component
        /// </summary>
        public string ComponentName { get; set; }
    }
}
