using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Specifies that this element depends on a component that must be rendered
    /// onto the page
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class NeedsComponentAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines a
        /// dependency on a componenr being on the page
        /// </summary>
        /// <param name="componentName">The name of the dependent component</param>
        public NeedsComponentAttribute(string componentName)
        {
            ComponentName = componentName;
        }

        /// <summary>
        /// The name of the required component
        /// </summary>
        public string ComponentName { get; set; }
    }
}
