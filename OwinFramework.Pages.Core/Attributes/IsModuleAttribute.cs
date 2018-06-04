using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone module that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsModuleAttribute: Attribute
    {
        /// <summary>
        /// Constructs an attribute that identifies a class as a module
        /// </summary>
        /// <param name="moduleName">The name of the module. Must be unique accross the 
        /// whole website</param>
        public IsModuleAttribute(string moduleName)
        {
            Name = moduleName;
        }

        /// <summary>
        /// Constructs an attribute that identifies a class as a module
        /// </summary>
        /// <param name="moduleName">The name of the module. Must be unique accross the 
        /// whole website</param>
        /// <param name="namespaceName">The namespace. Must ve a valid JavaScript identifier
        /// and css class name</param>
        public IsModuleAttribute(string moduleName, string namespaceName)
        {
            Name = moduleName;
            Namespace = namespaceName;
        }

        /// <summary>
        /// Defines an optional name for this module so that is can be referenced 
        /// by name in other elements
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defines the namespace for all JavaScript functions and css classes.
        /// Defaults to the name of the module if not specified
        /// </summary>
        public string Namespace { get; set; }
    }
}
