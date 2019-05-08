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
        /// Specifies that the region contains a layout defined in 
        /// the CMS
        /// </summary>
        [Mapping("layoutId")]
        public long? LayoutId { get; set; }

        /// <summary>
        /// Specifies that the region contains a component defined in
        /// code and referenced by name
        /// </summary>
        [Mapping("componentName")]
        public string ComponentName { get; set; }

        /// <summary>
        /// Specifies that the region contains a component defined in 
        /// the CMS
        /// </summary>
        [Mapping("componentId")]
        public long? ComponentId { get; set; }

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
        /// Optional ID of the type of data to repeat in this region
        /// </summary>
        [Mapping("repeatDataTypeId")]
        public long? RepeatDataTypeId { get; set; }

        /// <summary>
        /// Optional ID of the scope to use when resolving the list of data to repeat
        /// </summary>
        [Mapping("repeatDataScopeId")]
        public long? RepeatDataScopeId { get; set; }

        /// <summary>
        /// Optional name of the scope to use when resolving the list of data to repeat
        /// </summary>
        [Mapping("repeatDataScopeName")]
        public string RepeatDataScopeName { get; set; }

        /// <summary>
        /// Optional ID of the scope to use for data that is repeated
        /// </summary>
        [Mapping("listDataScopeId")]
        public long? ListDataScopeId { get; set; }

        /// <summary>
        /// Optional name of the scope to use for data that is repeated
        /// </summary>
        [Mapping("listDataScopeName")]
        public string ListDataScopeName { get; set; }

        /// <summary>
        /// Optional HTML tag to wrap repeated child elements in
        /// </summary>
        [Mapping("listElementTag")]
        public string ListElementTag { get; set; }

        /// <summary>
        /// Optional HTML style to apply to child elements
        /// </summary>
        [Mapping("listElementStyle")]
        public string ListElementStyle { get; set; }

        /// <summary>
        /// Optional comma separated list of CSS class names to apply to child elements
        /// </summary>
        [Mapping("listElementStyle")]
        public string ListElementClasses { get; set; }

        /// <summary>
        /// When the region contains a layout this property can be used to
        /// override the zones of the layout for this region version.
        /// </summary>
        public LayoutZoneRecord[] LayoutZones { get; set; }

        /// <summary>
        /// A list of the templates to render into this region.
        /// </summary>
        public RegionTemplateRecord[] RegionTemplates { get; set; }

        /// <summary>
        /// A list of the components to render directly onto any page that 
        /// includes this region. 
        /// These are typically non-visual components that do things like 
        /// render references to JavaScript libraries into the head of the page.
        /// </summary>
        public ElementComponentRecord[] Components { get; set; }

        /// <summary>
        /// A list of the data scopes to use to resolve data binding within this region
        /// </summary>
        public ElementDataScopeRecord[] DataScopes { get; set; }
    }
}
