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
        public IsLayoutAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Names this layout so that is can be referenced by name in other elements
        /// </summary>
        public string Name { get; set; }
    }
}
