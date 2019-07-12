using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of environment records
    /// </summary>
    public class EnvironmentRecord: RecordBase
    {
        public EnvironmentRecord()
        {
            RecordType = "Environment";
        }

        /// <summary>
        /// Primary key that uniquely identifies this environment
        /// </summary>
        [Mapping("environmentId")]
        [JsonProperty("environmentId")]
        public long EnvironmentId { get; set; }

        /// <summary>
        /// This is used to construct the Canonical URL of pages in the website
        /// for this environment
        /// </summary>
        [Mapping("baseUrl")]
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }

        /// <summary>
        /// This defines which version of the website should be displayed in
        /// this environment. Changing this value caused the website to get
        /// rebuilt using a different version of the website.
        /// </summary>
        [Mapping("websiteVersionId")]
        [JsonProperty("websiteVersionId")]
        public long WebsiteVersionId { get; set; }
    }
}
