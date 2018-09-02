using System.Xml.Serialization;
using System.Linq;
using Newtonsoft.Json;
using OwinFramework.Pages.Core.Interfaces;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Base class for used to gather debugging information
    /// </summary>
    public class DebugInfo
    {
        /// <summary>
        /// The name of the component
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of component
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The object that produced this debug information
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public object Instance { get; set; }

        /// <summary>
        /// The element that this debug info relates to
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public IElement Element;


        /// <summary>
        /// Information about data required by this element
        /// </summary>
        public DebugDataConsumer DataConsumer { get; set; }

        /// <summary>
        /// The components that this element depends on
        /// </summary>
        [JsonProperty, XmlIgnore]
        public List<IComponent> DependentComponents { get; set; }

        /// <summary>
        /// The parent of this element or null if this is the page
        /// </summary>
        public DebugInfo Parent;

        /// <summary>
        /// The children of this element if any
        /// </summary>
        public List<DebugInfo> Children;

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugInfo()
        {
            Type = GetType().Name.Aggregate(string.Empty, (s, c) => s + (char.IsUpper(c) ? " " + char.ToLower(c) : c.ToString())).Trim();
        }

        /// <summary>
        /// Returns the default description
        /// </summary>
        public override string ToString()
        {
            var result = Type.ToLower();

            if (!string.IsNullOrEmpty(Name))
                result += " '" + Name + "'";

            return result;
        }
    }
}
