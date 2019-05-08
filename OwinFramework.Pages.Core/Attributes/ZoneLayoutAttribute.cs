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
        /// <param name="region">The name of the region to populate</param>
        /// <param name="layout">The name of the layout to render in this region</param>
        public ZoneLayoutAttribute(string region, string layout)
        {
            Region = region;
            Layout = layout;
        }

        /// <summary>
        /// The name of the region to populate
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// The name of the layout to place in this region
        /// </summary>
        public string Layout { get; set; }
    }
}
