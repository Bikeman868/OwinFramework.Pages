using System;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate
{
    /// <summary>
    /// These records are send when the user changes which version of
    /// an element a specific version of the website should use.
    /// </summary>
    [Serializable]
    public class WebsiteVersionChange
    {
        /// <summary>
        /// The version of the website affected by this change
        /// </summary>
        public long WebsiteVersionId { get; set; }

        /// <summary>
        /// The type of element whose version was changed, can
        /// be 'Layout', 'Component', 'Region' etc
        /// </summary>
        public string ElementType { get; set; }

        /// <summary>
        /// The database ID of the element that was switched
        /// </summary>
        public long ElementId { get; set; }

        /// <summary>
        /// The unique id of the element version that was being 
        /// used on this version of the website or null if this
        /// is a new element for this version of the website
        /// </summary>
        public long? OldElementVersionId { get; set; }

        /// <summary>
        /// The unique id of the element version that should noe
        /// be used  on this version of the website or null if this
        /// element was removed from this version of the website
        /// </summary>
        public long? NewElementVersionId { get; set; }
    }
}