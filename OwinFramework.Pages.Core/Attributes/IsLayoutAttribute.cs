using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone layout that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsLayoutAttribute: Attribute
    {
        /// <summary>
        /// Constructs an attribute that defines a class to be a layout
        /// </summary>
        /// <param name="name">The name of the layout. Must be unique within a package</param>
        /// <param name="regionNesting">Specifies how regions are places within each other. 
        /// Follow region name with round brackets containing sub-regions. Use commas to
        /// separate sibling regions. For example "region1(region2,region3(region4,region5)).region6"
        /// specifies that the layout directly contains region1 and region6 and that region1
        /// contains region2 and region 3, and that region3 further contains region 4 and region5</param>
        public IsLayoutAttribute(string name, string regionNesting)
        {
            Name = name;
            RegionNesting = regionNesting;
        }

        /// <summary>
        /// Names this layout so that is can be referenced by name in other elements
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Names this layout so that is can be referenced by name in other elements
        /// </summary>
        public string RegionNesting { get; set; }
    }
}
