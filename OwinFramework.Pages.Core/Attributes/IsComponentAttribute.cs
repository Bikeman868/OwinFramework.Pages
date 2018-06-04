using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone component that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsComponentAttribute: Attribute
    {
        /// <summary>
        /// Constructs an attribute that defines a class to be a component
        /// </summary>
        /// <param name="name">The name of the component. Must be unique within a package</param>
        public IsComponentAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Names for this component so that is can be referenced by name in other elements
        /// </summary>
        public string Name { get; set; }
    }
}
