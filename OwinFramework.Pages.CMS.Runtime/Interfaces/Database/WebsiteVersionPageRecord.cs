using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class WebsiteVersionPageRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this version of the website
        /// </summary>
        [Mapping("websiteVersionId")]
        [JsonProperty("websiteVersionId")]
        public long WebsiteVersionId { get; set; }

        /// <summary>
        /// The ID of a page to include in this version of the website
        /// </summary>
        [Mapping("pageId")]
        [JsonProperty("pageId")]
        public long PageId { get; set; }

        /// <summary>
        /// Defines which version of the page should be used in this version
        /// of the website
        /// </summary>
        [Mapping("pageVersionId")]
        [JsonProperty("pageVersionId")]
        public long PageVersionId { get; set; }
    }
}
