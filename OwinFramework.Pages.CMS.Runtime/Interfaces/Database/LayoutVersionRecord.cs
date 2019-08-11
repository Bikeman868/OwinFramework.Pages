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
        /// Specifies the defalt content for each zone named in the zoneNesting
        /// property. These zone assignments can be overriden for a page.
        /// </summary>
        [JsonProperty("layoutZones")]
        public LayoutZoneRecord[] LayoutZones { get; set; }

        /// <summary>
        /// A list of the components to render directly onto any page containing this layout.
        /// These are typically non-visual components that do things like 
        /// render references to JavaScript libraries into the head of the page.
        /// </summary>
        [JsonProperty("components")]
        public ElementComponentRecord[] Components { get; set; }
    }
}
