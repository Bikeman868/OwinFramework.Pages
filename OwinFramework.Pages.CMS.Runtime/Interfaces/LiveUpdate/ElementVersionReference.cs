using System;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate
{
    /// <summary>
    /// This is used to identify a specific element version
    /// </summary>
    [Serializable]
    public class ElementVersionReference
    {
        /// <summary>
        /// The element type. Can be 'Layout', 'Region' etc
        /// </summary>
        public string ElementType { get; set; }

        /// <summary>
        /// The unique ID of the element
        /// </summary>
        public long ElementId { get; set; }

        /// <summary>
        /// The unique ID of the element version
        /// </summary>
        public long ElementVersionId { get; set; }
    }
}