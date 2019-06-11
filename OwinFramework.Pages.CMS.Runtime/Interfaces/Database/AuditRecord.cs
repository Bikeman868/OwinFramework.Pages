using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of component records
    /// </summary>
    public class AuditRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this audit record in the database
        /// </summary>
        [Mapping("id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Foreign key that uniquely identifies the element version that this 
        /// audit entry is for or null if this is not element related
        /// </summary>
        [Mapping("elementVersionId")]
        [JsonProperty("elementVersionId")]
        public long? ElementVersionId { get; set; }

        /// <summary>
        /// The identity of the user/application/system that modified this element version
        /// </summary>
        [Mapping("modifiedBy")]
        [JsonProperty("modifiedBy")]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The date/time when this element was modified
        /// </summary>
        [Mapping("modifiedWhen")]
        [JsonProperty("modifiedWhen")]
        public DateTime ModifiedWhen { get; set; }

        /// <summary>
        /// This field contains a JSON serialization of a data structure that describes
        /// the changes that the user made to the element version
        /// </summary>
        [Mapping("changeDetails")]
        [JsonProperty("changeDetails")]
        public string ChangeDetails { get; set; }
    }
}
