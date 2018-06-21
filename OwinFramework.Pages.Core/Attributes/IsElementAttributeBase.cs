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
        /// <param name="dataScope">The scope name used to resolve data providers</param>
        public IsElementAttributeBase(string name, string dataScope = null)
        {
            Name = name;
            DataScope = dataScope;
        }

        /// <summary>
        /// Names for this element so that is can be referenced by name in other elements
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the name of the data context handler to use to construct a child
        /// context for this element. If you do not set this property then the context is
        /// inherited from the parent container.
        /// </summary>
        public string DataScope { get; set; }
    }
}
