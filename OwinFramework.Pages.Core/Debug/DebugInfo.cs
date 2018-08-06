using System.Xml.Serialization;
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
        /// Information about data required by this element
        /// </summary>
        public DebugDataConsumer DataConsumer { get; set; }

        /// <summary>
        /// The components that this element depends on
        /// </summary>
        public List<IComponent> DependentComponents { get; set; }

        /// <summary>
        /// The live instance that this is debug info for
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public object Instance { get; set; }

        /// <summary>
        /// Returns the default description
        /// </summary>
        public override string ToString()
        {
            var result = Type;

            if (!string.IsNullOrEmpty(Name))
                result += " '" + Name + "'";

            return result;
        }
    }
}
