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
        public long WebsiteVersionId { get; set; }

        /// <summary>
        /// The segment of users that see this version
        /// </summary>
        [Mapping("segment")]
        public string UserSegment { get; set; }
    }
}
