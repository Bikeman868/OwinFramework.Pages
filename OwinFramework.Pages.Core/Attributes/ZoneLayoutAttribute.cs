using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to populate a layout zone with a layout
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ZoneLayoutAttribute: Attribute
    {
        /// <summary>
        /// Creates an attribute that associates a layout with a region of another layout
        /// </summary>
        /// <param name="zoneName">The name of the zone to populate</param>
        /// <param name="layoutName">The name of the layout to render in this region</param>
        public ZoneLayoutAttribute(string zoneName, string layoutName)
        {
            ZoneName = zoneName;
            LayoutName = layoutName;
        }

        /// <summary>
        /// The name of the zone to populate
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// The name of the layout to place in this region
        /// </summary>
        public string LayoutName { get; set; }
    }
}
