using System;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields common to all elements
    /// </summary>
    public class ElementRecordBase: RecordBase
    {
        /// <summary>
        /// Primary key that uniquely identifies this element in the database. All elements
        /// have unique ids. For example there are no pages with the same ID as a layout etc.
        /// This is done so that audit records for example do not need to know the element type.
        /// </summary>
        [Mapping("elementId")]
        public long ElementId { get; set; }

        /// <summary>
        /// This can be used to determine which table to join to pull out the rest
        /// of the element details. This is cumbersome but rarely required. Most often
        /// you will start from a specific element type and join to the shared element
        /// details table instead.
        /// </summary>
        [Mapping("elementType")]
        public string ElementType { get; set; }
    }
}
