using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    public class HistorySummaryRecord
    {
        /// <summary>
        /// The identity that made these changes
        /// </summary>
        [Mapping("id")]
        [JsonProperty("id")]
        public long SummaryId { get; set; }

        /// <summary>
        /// The date when these events occured
        /// </summary>
        [Mapping("when")]
        [JsonProperty("when")]
        public DateTime When { get; set; }

        /// <summary>
        /// The identity that made these changes
        /// </summary>
        [Mapping("identity")]
        [JsonProperty("identity")]
        public string Identity { get; set; }

        /// <summary>
        /// A summary of the changes that were made. Details can be obtained
        /// using the SummaryId to requiest the individual change events
        /// </summary>
        [Mapping("changes")]
        [JsonProperty("changes")]
        public string ChangeSummary { get; set; }
    }
}
