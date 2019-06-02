using OwinFramework.Pages.Core.Enums;
using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a service to generate a component that
    /// will render JavaScript into the page that allows the service endpoints
    /// to be called easily.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GenerateClientScriptAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that tells the service builder
        /// to also build a component that will render a client-side wrapper for the
        /// service.
        /// </summary>
        /// <param name="componentName">The name of the component to generate. Include this
        /// component on pages that make calls to this service</param>
        public GenerateClientScriptAttribute(string componentName)
        {
            ComponentName = componentName;
        }

        /// <summary>
        /// The name of the component to generate. Including this component on a 
        /// page will render JavaScript into that page that allows the service
        /// endpoints to be called easily
        /// </summary>
        public string ComponentName { get; set; }
    }
}
