using System.Collections.Generic;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of region records.
    /// Note that you can only render one thing into the region (layout,
    /// component, asset or template set). You can not specify a layout 
    /// name and a component name etc.
    /// </summary>
    public class RegionVersionRecord: ElementVersionRecordBase
    {
        /// <summary>
        /// Specifies that the region contains a layout defined in code and
        /// referenced by name
        /// </summary>
        [Mapping("layoutName")]
        public string LayoutName { get; set; }

        /// <summary>
        /// Specifies that the region contains a layout version defined in 
        /// the CMS
        /// </summary>
        [Mapping("layoutVersionId")]
        public long? LayoutVersionId { get; set; }

        /// <summary>
        /// Specifies that the region contains a component. Components must be 
        /// defined in code and referenced by name
        /// </summary>
        [Mapping("componentName")]
        public string ComponentName { get; set; }

        /// <summary>
        /// Specifies that the region contains localizable html. This 
        /// property contains the asset name of the text asset to localize
        /// </summary>
        [Mapping("assetName")]
        public string AssetName { get; set; }

        /// <summary>
        /// Specifies default Html to display when there is no localized
        /// version of the asset that matches the browser locale.
        /// </summary>
        [Mapping("assetValue")]
        public string AssetValue { get; set; }

        /// <summary>
        /// When the region contains a layout this property can be used to
        /// override the regions of the layout for this region version.
        /// </summary>
        public List<LayoutRegionRecord> LayoutRegions { get; set; }

        /// <summary>
        /// A list of the templates to render into this region.
        /// </summary>
        public List<RegionTemplateRecord> RegionTemplates { get; set; }

        /// <summary>
        /// A list of the components to render directly onto any page that 
        /// includes this region. 
        /// These are typically non-visual components that do things like 
        /// render references to JavaScript libraries into the head of the page.
        /// </summary>
        public List<ElementComponentRecord> Components { get; set; }
    }
}
