using System;
using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate
{
    /// <summary>
    /// This is used to identify a specific element
    /// </summary>
    public class ElementReference
    {
        /// <summary>
        /// The element type. Can be 'Layout', 'Region' etc
        /// </summary>
        [JsonProperty("elementType")]
        public string ElementType { get; set; }

        /// <summary>
        /// The unique ID of the element
        /// </summary>
        [JsonProperty("id")]
        public long ElementId { get; set; }
    }
}