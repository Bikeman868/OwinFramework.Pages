﻿using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone service that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsServiceAttribute: Attribute
    {
        /// <summary>
        /// Defines an optional name for this service so that is can be referenced 
        /// by name in other elements
        /// </summary>
        public string Name { get; set; }
    }
}