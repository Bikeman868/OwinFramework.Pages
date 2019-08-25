using System;
using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate
{
    /// <summary>
    /// The represents a single property value change on a specific
    /// version of an element
    /// </summary>
    public class PropertyChange
    {
        /// <summary>
        /// The element type. Can be 'Layout', 'Region' etc
        /// </summary>
        [JsonProperty("recordType")]
        public string RecordType { get; set; }

        /// <summary>
        /// The unique ID of the element or element version affected
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// The name of the property that was changed. This is
        /// the C# identifier for the property on the class or
        /// interface
        /// </summary>
        [JsonProperty("name")]
        public string PropertyName { get; set; }

        /// <summary>
        /// If this property is an array then this property can
        /// contain the index of the array element to update. The
        /// value to update will either be passed in the PropertyValue
        /// or PropertyObject (arrays of arrays are not supported)
        /// </summary>
        [JsonProperty("index")]
        public int? ArrayIndex { get; set; }

        /// <summary>
        /// The new value for this property. For complex properties
        /// this will be a JSON serialization.
        /// </summary>
        [JsonProperty("value")]
        public string PropertyValue { get; set; }

        /// <summary>
        /// If this property is an array and the whole array has changed
        /// then this contains the new array value
        /// </summary>
        [JsonProperty("arrayValue")]
        public dynamic[] PropertyArray { get; set; }

        /// <summary>
        /// If this object is a property and the entire object is different
        /// then this is the new object to store
        /// </summary>
        [JsonProperty("objectValue")]
        public dynamic PropertyObject { get; set; }
    }
}