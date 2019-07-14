using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of history events
    /// </summary>
    public class HistoryEventRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this event record in the database
        /// </summary>
        [Mapping("id")]
        [JsonProperty("id")]
        public long EventId { get; set; }

        /// <summary>
        /// Foreign key that identifies the record where this event is summarized
        /// </summary>
        [Mapping("summaryId")]
        [JsonProperty("summaryId")]
        public long SummaryId { get; set; }

        /// <summary>
        /// The type of record that was changed
        /// </summary>
        [Mapping("recordType")]
        [JsonProperty("recordType")]
        public string RecordType { get; set; }

        /// <summary>
        /// The unique ID of the record that was changed
        /// </summary>
        [Mapping("recordId")]
        [JsonProperty("recordId")]
        public long RecordId { get; set; }

        /// <summary>
        /// The identity of the user/application/system that changed this record
        /// </summary>
        [Mapping("identity")]
        [JsonProperty("identity")]
        public string Identity { get; set; }

        /// <summary>
        /// The date/time when this element was changed
        /// </summary>
        [Mapping("when")]
        [JsonProperty("when")]
        public DateTime When { get; set; }

        /// <summary>
        /// This field contains a JSON serialization of a data structure that describes
        /// the changes that the user made to the record
        /// </summary>
        [Mapping("changeDetails")]
        [JsonProperty("changeDetails")]
        public string ChangeDetails { get; set; }
    }
}
