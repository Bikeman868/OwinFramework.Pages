using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class WebsiteVersionLayoutRecord: WebsiteVersionRecordBase
    {
        /// <summary>
        /// The unique ID of a layout to include in this version of the website
        /// </summary>
        [Mapping("layoutId")]
        [JsonProperty("layoutId")]
        public long LayoutId { get; set; }

        /// <summary>
        /// Defines which version of this layout should be used in this 
        /// version of the website
        /// </summary>
        [Mapping("layoutVersionId")]
        [JsonProperty("layoutVersionId")]
        public long LayoutVersionId { get; set; }
    }
}
