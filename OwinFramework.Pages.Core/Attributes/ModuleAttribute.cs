using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone module that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ModuleAttribute: Attribute
    {
        /// <summary>
        /// Defines an optional name for this module so that is can be referenced 
        /// by name in other elements
        /// </summary>
        public string Name { get; set; }
    }
}
