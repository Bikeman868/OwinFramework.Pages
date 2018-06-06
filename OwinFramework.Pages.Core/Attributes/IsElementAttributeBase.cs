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
        /// <param name="dataContext">The name of the context handler to run to establish this
        /// elements data context</param>
        public IsElementAttributeBase(string name, string dataContext = null)
        {
            Name = name;
            DataContext = dataContext;
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
        public string DataContext { get; set; }
    }
}
