using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields common to all elements
    /// </summary>
    public class ElementRecordBase: RecordBase
    {
        /// <summary>
        /// Primary key that uniquely identifies this element in the database. All elements
        /// have unique ids. For example there are no pages with the same ID as a layout etc.
        /// This is done so that audit records for example do not need to know the element type.
        /// </summary>
        [Mapping("elementId")]
        [JsonProperty("elementId")]
        public long ElementId { get; set; }
    }
}
