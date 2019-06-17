﻿using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// The represents a single property value change on a specific
    /// version of an element
    /// </summary>
    public class PropertyChange
    {
        /// <summary>
        /// The name of the property that was changed. This is
        /// the C# identifier for the property on the class or
        /// interface
        /// </summary>
        [JsonProperty("name")]
        public string PropertyName { get; set; }

        /// <summary>
        /// The new value for this property. For complex properties
        /// this will be a JSON serialization.
        /// </summary>
        [JsonProperty("value")]
        public string PropertyValue { get; set; }
    }
}