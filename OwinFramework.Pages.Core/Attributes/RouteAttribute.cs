﻿using System;
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
        public RouteAttribute(string path, params Methods[] methods)
        {
            Path = path;
            Methods = methods;
        }

        /// <summary>
        /// The path to route to this page or service
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Names for this component so that is can be referenced by name in other elements
        /// </summary>
        public Methods[] Methods { get; set; }
    }
}
