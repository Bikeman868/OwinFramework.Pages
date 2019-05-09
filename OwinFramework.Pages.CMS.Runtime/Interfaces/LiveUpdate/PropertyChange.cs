using System;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate
{
    /// <summary>
    /// The represents a single property value change on a specific
    /// version of an element
    /// </summary>
    [Serializable]
    public class PropertyChange
    {
        /// <summary>
        /// The element type. Can be 'Layout', 'Region' etc
        /// </summary>
        public string ElementType { get; set; }

        /// <summary>
        /// The unique ID of the element version affected
        /// </summary>
        public long ElementVersionId { get; set; }

        /// <summary>
        /// The name of the property that was changed. This is
        /// the C# identifier for the property on the class or
        /// interface
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The new value for this property. For complex properties
        /// this will be a JSON serialization.
        /// </summary>
        public string PropertyValue { get; set; }
    }
}