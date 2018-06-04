using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to populate a region with a component
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RegionComponentAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defined the component
        /// to place in a region of a layout
        /// </summary>
        /// <param name="region"></param>
        /// <param name="component"></param>
        public RegionComponentAttribute(string region, string component)
        {
            Region = region;
            Component = component;
        }

        /// <summary>
        /// The name of the region to populate
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// The name of the component to place in this region
        /// </summary>
        public string Component { get; set; }
    }
}
