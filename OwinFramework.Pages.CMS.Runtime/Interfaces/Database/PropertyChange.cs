using Newtonsoft.Json;

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
        /// If this property is an array then this property can
        /// contain the index of the array element to update. The
        /// value to update will either be passed in the PropertyValue
        /// or PropertyObject
        /// </summary>
        [JsonProperty("index")]
        public int? ArrayIndex { get; set; }

        /// <summary>
        /// The new value for this property. If this property is not
        /// null and the property is an array then the ArrayIndex must
        /// be set.
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

        /// <summary>
        /// This is sent when only some of the object properties were changed
        /// </summary>
        [JsonProperty("changedProperties")]
        public dynamic ObjectProperties { get; set; }
    }
}