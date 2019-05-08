using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a layout that you want to have discovered and 
    /// registered automitically at startup. If your layout implements ILayout
    /// it works out better if you build and register it using a Package instead
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsLayoutAttribute : IsElementAttributeBase
    {
        /// <summary>
        /// Constructs an attribute that defines a class to be a layout
        /// </summary>
        /// <param name="name">The name of the layout. Must be unique within a package</param>
        /// <param name="zoneNesting">Specifies how zones are places within each other. 
        /// Follow zone name with round brackets containing child zones. Use commas to
        /// separate sibling zones. For example "zone1(zone2,zone3(zone4,zone5))zone6"
        /// specifies that the layout directly contains zone1 and zone6 and that zone1
        /// contains zone2 and zone3, and that zone3 further contains zone4 and zone5</param>
        public IsLayoutAttribute(string name, string zoneNesting)
            : base(name)
        {
            Name = name;
            ZoneNesting = zoneNesting;
        }

        /// <summary>
        /// Names this layout so that is can be referenced by name in other elements
        /// </summary>
        public string ZoneNesting { get; set; }
    }
}
