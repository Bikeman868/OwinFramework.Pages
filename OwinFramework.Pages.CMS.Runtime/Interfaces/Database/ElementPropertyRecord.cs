using System;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the properties of a component. These correspond
    /// with the C# property definitions on the class that implements the
    /// component.
    /// </summary>
    public class ElementPropertyRecord: RecordBase
    {
        /// <summary>
        /// Primary key that uniquely identifies this element property in the database.
        /// </summary>
        [Mapping("elementPropertyId")]
        public long ElementPropertyId { get; set; }

        /// <summary>
        /// The element that has this property available
        /// </summary>
        [Mapping("elementId")]
        public long ElementId { get; set; }

        /// <summary>
        /// The name of a region defined in code that is an editor for this
        /// property. This region will be renderd onto the editor and displayed
        /// when the user clicks the edit button next to this field. If this
        /// is not specified then the CMS will provide an editor based on the 
        /// Type.
        /// </summary>
        [Mapping("editRegionName")]
        public string EditRegionName { get; set; }

        /// <summary>
        /// The name of a region defined in code that displays the current value
        /// of this property. When this is not specified the CMS will provide
        /// a display UI based on the Type.
        /// </summary>
        [Mapping("displayRegionName")]
        public string DisplayRegionName { get; set; }

        /// <summary>
        /// The fully qualified name of the .Net type to use for
        /// parsing property values
        /// </summary>
        [Mapping("typeName")]
        public string TypeName { get; set; }

        /// <summary>
        /// The .Net type of this property
        /// </summary>
        public Type Type { get; set; }
    }
}
