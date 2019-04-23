using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a page or service to route requests to it
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RouteAttribute: Attribute
    {
        /// <summary>
        /// Constructs an attribute that defines a class to be a component
        /// </summary>
        /// <param name="path">The URL path that should be routed. Can include widecards</param>
        /// <param name="methods">The list of http methods that should be routed</param>
        public RouteAttribute(string path, params Method[] methods)
        {
            Path = path;
            Methods = methods;
        }

        /// <summary>
        /// The path to route to this page or service
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Request methods that should be routed to this page or service
        /// </summary>
        public Method[] Methods { get; set; }

        /// <summary>
        /// Routes are compared to the request in priority order and the 
        /// first matching route is used to handle the request
        /// </summary>
        public int Priority { get; set; }
    }
}
