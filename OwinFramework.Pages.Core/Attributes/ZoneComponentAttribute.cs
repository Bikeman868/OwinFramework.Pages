using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to populate a layout zoneName with a component
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ZoneComponentAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines the component
        /// to place in a zoneName of a layout
        /// </summary>
        public ZoneComponentAttribute(string zoneName, string component)
        {
            ZoneName = zoneName;
            ComponentName = component;
        }

        /// <summary>
        /// The name of the zoneName to populate
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// The name of the component to place in this zoneName
        /// </summary>
        public string ComponentName { get; set; }
    }
}
