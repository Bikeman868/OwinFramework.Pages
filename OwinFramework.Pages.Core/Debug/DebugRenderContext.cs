using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about a layout
    /// </summary>
    public class DebugRenderContext : DebugInfo
    {
        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugRenderContext()
        {
            Type = "Render context";
        }

        /// <summary>
        /// Debug information about the data that will be 
        /// used for data binding during this rendering operation
        /// </summary>
        [JsonProperty, XmlIgnore]
        public IDictionary<int, DebugDataContext> Data { get; set; }
    }
}
