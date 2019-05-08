using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class WebsiteVersionLayoutRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this version of the website
        /// </summary>
        [Mapping("websiteVersionId")]
        public long WebsiteVersionId { get; set; }

        /// <summary>
        /// The unique ID of a layout to include in this version of the website
        /// </summary>
        [Mapping("layoutId")]
        public long LayoutId { get; set; }

        /// <summary>
        /// Defines which version of this layout should be used in this 
        /// version of the website
        /// </summary>
        [Mapping("layoutVersionId")]
        public long LayoutVersionId { get; set; }
    }
}
