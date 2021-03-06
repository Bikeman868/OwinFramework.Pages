﻿using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    public class ElementComponentRecord
    {
        /// <summary>
        /// The ID of the elemment that this component should be rendered onto
        /// </summary>
        [Mapping("elementVersionId")]
        [JsonProperty("elementVersionId")]
        public long ElementVersionId { get; set; }

        /// <summary>
        /// The name of a component to render on the page
        /// </summary>
        [Mapping("component")]
        [JsonProperty("component")]
        public string ComponentName { get; set; }
    }
}
