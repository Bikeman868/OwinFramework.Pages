using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class WebsiteVersionRegionRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this version of the website
        /// </summary>
        [Mapping("websiteVersionId")]
        public long WebsiteVersionId { get; set; }

        /// <summary>
        /// The unique ID of a region to include in this version of the website
        /// </summary>
        [Mapping("regionId")]
        public long RegionId { get; set; }

        /// <summary>
        /// Defines which version of the region should be used by this version 
        /// of the website
        /// </summary>
        [Mapping("regionVersionId")]
        public long RegionVersionId { get; set; }
    }
}
