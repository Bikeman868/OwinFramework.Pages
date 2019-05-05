using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class WebsiteVersionDataTypeRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this version of the website
        /// </summary>
        [Mapping("websiteVersionId")]
        public long WebsiteVersionId { get; set; }

        /// <summary>
        /// The unique ID of a data type to include in this version of the website
        /// </summary>
        [Mapping("dataTypeId")]
        public long DataTypeId { get; set; }

        /// <summary>
        /// Defines which version of the data type should be used by this version 
        /// of the website
        /// </summary>
        [Mapping("dataTypeVersionId")]
        public long DataTypeVersionId { get; set; }
    }
}
