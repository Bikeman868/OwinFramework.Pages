using System;
using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate
{
    /// <summary>
    /// This is used to identify a specific element
    /// </summary>
    public class RecordReference
    {
        /// <summary>
        /// The element type. Can be 'Layout', 'Region' etc
        /// </summary>
        [JsonProperty("recordType")]
        public string RecordType { get; set; }

        /// <summary>
        /// The unique ID of the record
        /// </summary>
        [JsonProperty("id")]
        public long ElementId { get; set; }
    }
}