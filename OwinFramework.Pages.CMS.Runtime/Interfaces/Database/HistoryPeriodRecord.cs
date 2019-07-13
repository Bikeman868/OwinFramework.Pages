using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// Represents changes that were made to a record over a period of time
    /// </summary>
    public class HistoryPeriodRecord
    {
        /// <summary>
        /// The type of record that this is a hsitory for
        /// </summary>
        [Mapping("recordType")]
        [JsonProperty("recordType")]
        public string RecordType { get; set; }

        /// <summary>
        /// The unique ID of the record that this is a history for
        /// </summary>
        [Mapping("recordId")]
        [JsonProperty("recordId")]
        public long RecordId { get; set; }

        /// <summary>
        /// The beginning of this period in the record's history or
        /// null if this history includes the record creation
        /// </summary>
        [Mapping("start")]
        [JsonProperty("start")]
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// The end of this period in the record's history or null
        /// if this period includes the record deletion event
        /// </summary>
        [Mapping("end")]
        [JsonProperty("end")]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// You can use this bookmark to request the next period in this
        /// record's history. This will be null if the end of the
        /// history has been reached
        /// </summary>
        [Mapping("bookmark")]
        [JsonProperty("bookmark")]
        public string Bookmark { get; set; }

        /// <summary>
        /// A summary of the events that occurred during this period
        /// in the record's history
        /// </summary>
        [Mapping("summaries")]
        [JsonProperty("summaries")]
        public HistorySummaryRecord[] Summaries { get; set; }
    }
}
