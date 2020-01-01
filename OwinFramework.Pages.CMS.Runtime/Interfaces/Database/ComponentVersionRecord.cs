using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of component version records
    /// </summary>
    public class ComponentVersionRecord: ElementVersionRecordBase
    {
        public const string RecordTypeName = "ComponentVersion";

        public ComponentVersionRecord()
        {
            RecordType = RecordTypeName;
        }

        /// <summary>
        /// The name of the component implementation. This is the name that
        /// the component is registered under with the name manager. The
        /// name can be qualified by a package namespace
        /// </summary>
        [JsonProperty("componentName")]
        public string ComponentName { get; set; }

        /// <summary>
        /// Specifies the properties that can be configured for each instance
        /// of this component
        /// </summary>
        [JsonProperty("properties")]
        public ElementPropertyRecord[] Properties { get; set; }
    }
}
