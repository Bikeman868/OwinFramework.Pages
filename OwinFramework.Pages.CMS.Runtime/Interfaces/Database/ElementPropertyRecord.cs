using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the properties of a component. These correspond
    /// with the C# property definitions on the class that implements the
    /// component.
    /// </summary>
    public class ElementPropertyRecord
    {
        /// <summary>
        /// Primary key that uniquely identifies this element property in the database.
        /// </summary>
        [Mapping("elementPropertyId")]
        public long Id { get; set; }

        /// <summary>
        /// The element version to apply this property value to
        /// </summary>
        [Mapping("elementVersionId")]
        public long ElementVersionId { get; set; }

        /// <summary>
        /// The name of the property value to set
        /// </summary>
        [Mapping("name")]
        public string Name { get; set; }

        /// <summary>
        /// The value to set for this property
        /// </summary>
        [Mapping("name")]
        public string Value { get; set; }
    }
}
