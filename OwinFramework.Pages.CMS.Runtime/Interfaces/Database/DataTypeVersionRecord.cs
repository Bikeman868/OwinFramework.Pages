using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// Used to expose a C# class to the CMS that can be selected
    /// as the type of data to repeat in a region
    /// </summary>
    public class DataTypeVersionRecord: ElementVersionRecordBase
    {
        /// <summary>
        /// The name of the .Net Assembly that contains this type
        /// </summary>
        [Mapping("assembly")]
        [JsonProperty("assembly")]
        public string AssemblyName { get; set; }

        /// <summary>
        /// The fully qualified name of the C# class
        /// </summary>
        [Mapping("typeName")]
        [JsonProperty("typeName")]
        public string TypeName { get; set; }

        /// <summary>
        /// The .Net Type that this record represents
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Ids of the data scope records for this type of data
        /// </summary>
        public long[] ScopeIds { get; set; }
    }
}
