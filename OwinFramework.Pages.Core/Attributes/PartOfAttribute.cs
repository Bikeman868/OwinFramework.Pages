using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to make it part of a module (namespace)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PartOfAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defined the component
        /// to place in a region of a page
        /// </summary>
        /// <param name="moduleName">The name of the module that this element belongs 
        /// to. Any JavaScript methods or css classes produced by this element
        /// will be in the namespace of this module, and can be aggregated into
        /// one css or js file</param>
        public PartOfAttribute(string moduleName)
        {
            ModuleName = moduleName;
        }

        /// <summary>
        /// The name of the layout
        /// </summary>
        public string ModuleName { get; set; }
    }
}
