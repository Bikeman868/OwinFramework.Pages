using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the data types that are needed to render an element
    /// </summary>
    public class ElementDataTypeRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this element in the database.
        /// </summary>
        [Mapping("recordId")]
        [JsonProperty("recordId")]
        public long RecordId { get; set; }

        /// <summary>
        /// The element version that this applies to
        /// </summary>
        [Mapping("elementVersionId")]
        [JsonProperty("elementVersionId")]
        public long ElementVersionId { get; set; }

        /// <summary>
        /// The ID of the data type that this element depends on
        /// </summary>
        [Mapping("dataTypeId")]
        [JsonProperty("dataTypeId")]
        public long DataTypeId { get; set; }
    }
}
