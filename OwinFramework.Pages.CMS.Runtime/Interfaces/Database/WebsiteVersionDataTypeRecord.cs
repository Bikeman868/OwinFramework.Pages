using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of website version data type records
    /// </summary>
    public class WebsiteVersionDataTypeRecord: WebsiteVersionRecordBase
    {
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
