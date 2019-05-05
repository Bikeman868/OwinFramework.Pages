using System.Collections.Generic;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class PageVersionRecord: ElementVersionRecordBase
    {
        /// <summary>
        /// When the layout is defined in code this is the name of the
        /// layout. Otherwise the LayoutVersionId determines the layout
        /// to use on this version of the page.
        /// </summary>
        [Mapping("layoutName")]
        public string LayoutName { get; set; }

        /// <summary>
        /// The ID of a layout or NULL if the layout is defined in code
        /// </summary>
        [Mapping("layoutVersionId")]
        public long? LayoutId { get; set; }

        /// <summary>
        /// When there are multiple ways to reach the same page you should
        /// set the canonical URL to avoid being penalized by search engines.
        /// When generating links within the website always use the canonical
        /// URL of the target page and not one of the aliases
        /// </summary>
        [Mapping("canonicalUrl")]
        public string CanonicalUrl { get; set; }

        /// <summary>
        /// The title of this page
        /// </summary>
        [Mapping("title")]
        public string Title { get; set; }

        /// <summary>
        /// A CSS style definition to render on the body tag for the page
        /// </summary>
        [Mapping("bodyStyle")]
        public string BodyStyle { get; set; }

        /// <summary>
        /// The name of a permission that is required to view this page
        /// </summary>
        [Mapping("permission")]
        public string RequiredPermission { get; set; }

        /// <summary>
        /// Optional asset path that is used to further qualify the
        /// security check.
        /// </summary>
        [Mapping("assetPath")]
        public string AssetPath { get; set; }

        /// <summary>
        /// Defines which request URLs will render this page
        /// </summary>
        public List<PageRouteRecord> Routes { get; set; }

        /// <summary>
        /// Overrides the content of the page layout just for this page
        /// </summary>
        public List<LayoutRegionRecord> LayoutRegions { get; set; }

        /// <summary>
        /// A list of the components to render directly onto the page. 
        /// These are typically non-visual components that do things like 
        /// render references to JavaScript libraries into the head of the page.
        /// </summary>
        public List<ElementComponentRecord> Components { get; set; }
    }
}
