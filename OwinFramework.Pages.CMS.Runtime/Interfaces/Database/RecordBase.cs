using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields common to all user created data
    /// </summary>
    public class RecordBase
    {
        /// <summary>
        /// This can be used to determine which table to join to pull out the rest
        /// of the record details. This is cumbersome but rarely required. Most often
        /// you will start from a specific record type and join to the shared record
        /// details table instead.
        /// </summary>
        [Mapping("recordType")]
        [JsonProperty("recordType")]
        public string RecordType { get; set; }

        /// <summary>
        /// The name of this record. This name must be unique amongst records of the
        /// same type.
        /// For element named this must be a valid CSS class name and a valid JavaScript identifier.
        /// The recommended format is all lower case with underscore between words - for example
        /// 'cart_checkout_1'
        /// </summary>
        [Mapping("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The name to show users in the CMS UI
        /// </summary>
        [Mapping("displayName")]
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Allows the user to enter an optional description to help them to remember
        /// why they created this element in the CMS editor
        /// </summary>
        [Mapping("description")]
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The identity of the user/application/system that created this element
        /// </summary>
        [Mapping("createdBy")]
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The date/time when this element was created. See the audit log for details
        /// of the changes that were made since then
        /// </summary>
        [Mapping("createdWhen")]
        [JsonProperty("createdWhen")]
        public DateTime CreatedWhen { get; set; }
    }
}
