using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the properties of a component. These correspond
    /// with the C# property definitions on the class that implements the
    /// component.
    /// </summary>
    public class ElementDataScopeRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this element property in the database.
        /// </summary>
        [Mapping("recordId")]
        [JsonProperty("recordId")]
        public long RecordId { get; set; }

        /// <summary>
        /// The element version to apply this property value to
        /// </summary>
        [Mapping("elementVersionId")]
        [JsonProperty("elementVersionId")]
        public long ElementVersionId { get; set; }

        /// <summary>
        /// The ID of the data scope to use when resolving data binding
        /// </summary>
        [Mapping("dataScopeId")]
        [JsonProperty("dataScopeId")]
        public long DataScopeId { get; set; }
    }
}
