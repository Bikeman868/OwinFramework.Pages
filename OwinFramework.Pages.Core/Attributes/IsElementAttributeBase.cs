using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Base class for defining element attributes
    /// </summary>
    public class IsElementAttributeBase: Attribute
    {
        /// <summary>
        /// Base class for attributes that define a class as an element
        /// </summary>
        /// <param name="name">The name of the element. Must be unique within a package and 
        /// element type</param>
        public IsElementAttributeBase(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Names for this element so that is can be referenced by name in other elements
        /// </summary>
        public string Name { get; set; }
    }
}
