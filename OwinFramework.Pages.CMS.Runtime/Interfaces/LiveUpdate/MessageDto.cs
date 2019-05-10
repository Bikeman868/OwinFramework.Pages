using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate
{
    /// <summary>
    /// This message is serialized and transmitted to other web servers so that
    /// they can update information in memory about the website design
    /// </summary>
    [JsonObject]
    public class MessageDto
    {
        /// <summary>
        /// This id can be used to ensure that duplicate messages are
        /// not processed twice
        /// </summary>
        [JsonProperty("id")]
        public Guid UniqueId { get; set; }

        /// <summary>
        /// The date/time in UTC when this message was generated. This can
        /// be useful for discarding stale messages
        /// </summary>
        [JsonProperty("when")]
        public DateTime WhenUtc { get; set; }

        /// <summary>
        /// The name of the machine that generated this message. Machines
        /// can use this to filter out their own messages from the update
        /// stream if they want
        /// </summary>
        [JsonProperty("machine")]
        public string MachineName { get; set; }

        /// <summary>
        /// The element properties that were changed by a user. Note that
        /// the database contains an audit trail that identifies who changed
        /// what when
        /// </summary>
        [JsonProperty("propertyChanges")]
        public List<PropertyChange> PropertyChanges { get; set; }

        /// <summary>
        /// A list of the elements whos live version was changed to a different
        /// version of the element on a specific version of the website
        /// </summary>
        [JsonProperty("websiteVersionChanges")]
        public List<WebsiteVersionChange> WebsiteVersionChanges { get; set; }

        /// <summary>
        /// A list of the elements that were added to the website
        /// </summary>
        [JsonProperty("newElements")]
        public List<ElementReference> NewElements { get; set; }

        /// <summary>
        /// A list of the elements that were deleted from the website
        /// </summary>
        [JsonProperty("deletedElements")]
        public List<ElementReference> DeletedElements { get; set; }

        /// <summary>
        /// A list of the element versions that were added to the website
        /// </summary>
        [JsonProperty("newVersions")]
        public List<ElementVersionReference> NewElementVersions { get; set; }

        /// <summary>
        /// A list of the element versions that were deleted from the website
        /// </summary>
        [JsonProperty("deletedElementVersions")]
        public List<ElementVersionReference> DeletedElementVersions { get; set; }
    }
}
