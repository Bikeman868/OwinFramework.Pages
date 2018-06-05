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
        /// Constructs and initializes an attribute that defines the region instance
        /// to place in a region of a layout
        /// </summary>
        /// <param name="regionName">The name of the region to populate within the layout</param>
        /// <param name="regionElement">The name of the region element to put into this region of the layout</param>
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
