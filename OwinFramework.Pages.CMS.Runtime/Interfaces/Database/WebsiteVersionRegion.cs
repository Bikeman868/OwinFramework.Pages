using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class WebsiteVersionRegionRecord: WebsiteVersionRecordBase
    {
        /// <summary>
        /// The unique ID of a region to include in this version of the website
        /// </summary>
        [Mapping("regionId")]
        [JsonProperty("regionId")]
        public long RegionId { get; set; }

        /// <summary>
        /// Defines which version of the region should be used by this version 
        /// of the website
        /// </summary>
        [Mapping("regionVersionId")]
        [JsonProperty("regionVersionId")]
        public long RegionVersionId { get; set; }
    }
}
