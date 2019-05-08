using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a layout to define its zones
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ZoneRegionAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines the region instance
        /// to place in a region of a layout
        /// </summary>
        /// <param name="zoneName">The name of the zone to populate within the layout</param>
        /// <param name="regionElementName">The name of the region element to put into this region of the layout</param>
        public ZoneRegionAttribute(string zoneName, string regionElementName)
        {
            ZoneName = zoneName;
            RegionElementName = regionElementName;
        }

        /// <summary>
        /// The name of the zone to populate
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// The name of the region element to place in this named region
        /// </summary>
        public string RegionElementName { get; set; }
    }
}
