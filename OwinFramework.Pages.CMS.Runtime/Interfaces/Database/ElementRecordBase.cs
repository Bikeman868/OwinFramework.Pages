using System;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields common to all elements
    /// </summary>
    public class ElementRecordBase
    {
        /// <summary>
        /// Primary key that uniquely identifies this element in the database. All elements
        /// have unique ids. For example there are no pages with the same ID as a layout etc.
        /// This is done so that audit records for example do not need to know the element type.
        /// </summary>
        [Mapping("elementId")]
        public long ElementId { get; set; }

        /// <summary>
        /// The name of this element. This name must be unique amongst elements of the
        /// same type, and it must be a valid CSS class name and a valid JavaScript identifier.
        /// The recommended format is all lower case with underscore between words - for example
        /// 'cart_checkout_1'
        /// </summary>
        [Mapping("name")]
        public string Name { get; set; }

        /// <summary>
        /// Allows the user to enter an optional description to help them to remember
        /// why they created this element in the CMS editor
        /// </summary>
        [Mapping("description")]
        public string Description { get; set; }

        /// <summary>
        /// The identity of the user/application/system that created this element
        /// </summary>
        [Mapping("createdBy")]
        public string CraetedBy { get; set; }

        /// <summary>
        /// The date/time when this element was created. See the audit log for details
        /// of the changes that were made since then
        /// </summary>
        [Mapping("createdWhen")]
        public DateTime CreatedWhen { get; set; }

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
