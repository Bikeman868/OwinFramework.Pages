using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of website version records
    /// </summary>
    public class WebsiteVersionRecord: RecordBase
    {
        [JsonProperty("elementType")]
        public string ElementType 
        { 
            get { return "WebsiteVersion"; }
            set { }
        }

        /// <summary>
        /// Primary key that uniquely identifies this version of the website
        /// </summary>
        [Mapping("websiteVersionId")]
        [JsonProperty("websiteVersionId")]
        public long WebsiteVersionId { get; set; }
    }
}
