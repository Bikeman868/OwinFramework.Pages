using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a layout to define its regions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class HasRegionAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defined the component
        /// to place in a region of a page
        /// </summary>
        /// <param name="regionName">The name of this region within the layout</param>
        /// <param name="regionElement">The name of the region element that manages this region</param>
        public HasRegionAttribute(string regionName, string regionElement)
        {
            RegionName = regionName;
            RegionElement = regionElement;
        }

        /// <summary>
        /// The name of the region to populate
        /// </summary>
        public string RegionName { get; set; }

        /// <summary>
        /// The name of the component to place in this region
        /// </summary>
        public string RegionElement { get; set; }
    }
}
