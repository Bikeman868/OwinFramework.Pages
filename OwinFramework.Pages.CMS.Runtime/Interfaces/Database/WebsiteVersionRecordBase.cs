using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields common to all website version records
    /// </summary>
    public class WebsiteVersionRecordBase
    {
        /// <summary>
        /// Primary key that uniquely identifies this version of the website
        /// </summary>
        [Mapping("websiteVersionId")]
        [JsonProperty("websiteVersionId")]
        public long WebsiteVersionId { get; set; }

        /// <summary>
        /// The name of the segmentation test scenario where this version mapping applies
        /// </summary>
        [Mapping("scenario")]
        [JsonProperty("scenario")]
        public string Scenario { get; set; }
    }
}
