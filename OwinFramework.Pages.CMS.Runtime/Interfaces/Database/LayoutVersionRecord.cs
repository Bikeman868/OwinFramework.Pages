using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class LayoutVersionRecord: ElementVersionRecordBase
    {
        public const string RecordTypeName = "LayoutVersion";

        public LayoutVersionRecord()
        {
            RecordType = RecordTypeName;
        }

        /// <summary>
        /// The names of the regions within this layout separated by commas. 
        /// Regions can be grouped by enclusing them in round brackets. For
        /// example "region1(region2,region3)"
        /// </summary>
        [Mapping("zoneNesting")]
        [JsonProperty("zoneNesting")]
        public string ZoneNesting { get; set; }

        /// <summary>
        /// The HTML tag to use as the outer container of the layout.
        /// Defaults to 'div' if this property is not set
        /// </summary>
        [Mapping("tag")]
        [JsonProperty("tag")]
        public string ContainerTag { get; set; }

        /// <summary>
        /// The inline style to apply to the layout container
        /// </summary>
        [Mapping("style")]
        [JsonProperty("style")]
        public string ContainerStyle { get; set; }

        /// <summary>
        /// The additional css class names to attach to the layout container
        /// </summary>
        [Mapping("classes")]
        [JsonProperty("classes")]
        public string ContainerClasses { get; set; }

        /// <summary>
        /// The HTML tag to place around nested zones.
        /// Defaults to 'div' if this property is not set
        /// </summary>
        [Mapping("nestingTag")]
        [JsonProperty("nestingTag")]
        public string NestingTag { get; set; }

        /// <summary>
        /// The inline style to apply to groups of nested zones
        /// </summary>
        [Mapping("nestingStyle")]
        [JsonProperty("nestingStyle")]
        public string NestingStyle { get; set; }

        /// <summary>
        /// The additional css class names to attach to groups of nested zones
        /// </summary>
        [Mapping("nestingClasses")]
        [JsonProperty("nestingClasses")]
        public string NestingClasses { get; set; }

        /// <summary>
        /// Specifies the defalt content for each zone named in the zoneNesting
        /// property. These zone assignments can be overriden where the layout
        /// is used (for example on a page)
        /// </summary>
        [JsonProperty("zones")]
        public LayoutZoneRecord[] Zones { get; set; }

        /// <summary>
        /// A list of the components to render directly onto any page containing this layout.
        /// These are typically non-visual components that do things like 
        /// render references to JavaScript libraries into the head of the page.
        /// </summary>
        [JsonProperty("components")]
        public ElementComponentRecord[] Components { get; set; }

        /// <summary>
        /// A list of data scopes to use when resolving data needs of this layout
        /// and all of the elements within it
        /// </summary>
        [JsonProperty("dataScopes")]
        public ElementDataScopeRecord[] DataScopes { get; set; }

        /// <summary>
        /// A list of data types that this layout needs to render
        /// </summary>
        [JsonProperty("dataTypes")]
        public ElementDataTypeRecord[] DataTypes { get; set; }
    }
}
