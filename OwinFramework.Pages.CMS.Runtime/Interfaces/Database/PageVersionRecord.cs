using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class PageVersionRecord: ElementVersionRecordBase
    {
        public PageVersionRecord()
        {
            RecordType = "PageVersion";
        }

        /// <summary>
        /// The PageId of a page to copy settings from where they
        /// are not defined within this page
        /// </summary>
        [Mapping("masterPageId")]
        [JsonProperty("masterPageId")]
        public long? MasterPageId { get; set; }

        /// <summary>
        /// When the layout is defined in code this is the name of the
        /// layout. Otherwise the LayoutVersionId determines the layout
        /// to use on this version of the page. If the layout id and the
        /// layout name are not defined then the layout is inherited from
        /// the master page.
        /// </summary>
        [Mapping("layoutName")]
        [JsonProperty("layoutName")]
        public string LayoutName { get; set; }

        /// <summary>
        /// The ID of a layout or NULL if the layout is defined in code
        /// </summary>
        [Mapping("layoutId")]
        [JsonProperty("layoutId")]
        public long? LayoutId { get; set; }

        /// <summary>
        /// When there are multiple ways to reach the same page you should
        /// set the canonical URL to avoid being penalized by search engines.
        /// When generating links within the website always use the canonical
        /// URL of the target page and not one of the aliases
        /// </summary>
        [Mapping("canonicalUrl")]
        [JsonProperty("canonicalUrl")]
        public string CanonicalUrl { get; set; }

        /// <summary>
        /// The title of this page
        /// </summary>
        [Mapping("title")]
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// A CSS style definition to render on the body tag for the page
        /// </summary>
        [Mapping("bodyStyle")]
        [JsonProperty("bodyStyle")]
        public string BodyStyle { get; set; }

        /// <summary>
        /// The name of a permission that is required to view this page
        /// </summary>
        [Mapping("permission")]
        [JsonProperty("permission")]
        public string RequiredPermission { get; set; }

        /// <summary>
        /// Optional asset path that is used to further qualify the
        /// security check.
        /// </summary>
        [Mapping("assetPath")]
        [JsonProperty("assetPath")]
        public string AssetPath { get; set; }

        /// <summary>
        /// Defines which request URLs will render this page
        /// </summary>
        [JsonProperty("routes")]
        public PageRouteRecord[] Routes { get; set; }

        /// <summary>
        /// Overrides the content of the page layout just for this page
        /// </summary>
        [JsonProperty("layoutZones")]
        public LayoutZoneRecord[] LayoutZones { get; set; }

        /// <summary>
        /// A list of the components to render directly onto the page. 
        /// These are typically non-visual components that do things like 
        /// render references to JavaScript libraries into the head of the page.
        /// </summary>
        [JsonProperty("components")]
        public ElementComponentRecord[] Components { get; set; }
    }
}
