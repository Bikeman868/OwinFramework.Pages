﻿using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the properties of a component. These correspond
    /// with the C# property definitions on the class that implements the
    /// component.
    /// </summary>
    public class ElementPropertyValueRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this element property in the database
        /// </summary>
        [Mapping("recordId")]
        [JsonProperty("recordId")]
        public long RecordId { get; set; }

        /// <summary>
        /// The element version to apply this property value to
        /// </summary>
        [Mapping("parentRecordId")]
        [JsonProperty("parentRecordId")]
        public long ParentRecordId { get; set; }

        /// <summary>
        /// The value text from the database to parse
        /// </summary>
        [Mapping("value")]
        [JsonProperty("value")]
        public string ValueText { get; set; }

        /// <summary>
        /// The value parsed from ValueText
        /// </summary>
        [JsonIgnore]
        public object Value { get; set; }
    }
}
