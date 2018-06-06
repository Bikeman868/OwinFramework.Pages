using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone component that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsComponentAttribute : IsElementAttributeBase
    {
        /// <summary>
        /// Constructs an attribute that defines a class to be a component
        /// </summary>
        /// <param name="name">The name of the component. Must be unique within a package</param>
        /// <param name="dataContext">The name of the context handler to run to establish this
        /// components data context</param>
        public IsComponentAttribute(string name, string dataContext = null)
            : base(name, dataContext)
        {
        }
    }
}
