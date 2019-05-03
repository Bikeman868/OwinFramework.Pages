using System.Collections.Generic;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class LayoutVersionRecord: ElementVersionRecordBase
    {
        /// <summary>
        /// The names of the regions within this layout separated by commas. 
        /// Regions can be grouped by enclusing them in round brackets. For
        /// example "region1(region2,region3)"
        /// </summary>
        [Mapping("regionNesting")]
        public string RegionNesting { get; set; }

        /// <summary>
        /// Specifies the defalt content for each region named in the RegionNesting
        /// property. These region assignments can be overriden for a page.
        /// </summary>
        public List<LayoutRegionRecord> LayoutRegions { get; set; }

        /// <summary>
        /// A list of the components to render directly onto any page containing this layout.
        /// These are typically non-visual components that do things like 
        /// render references to JavaScript libraries into the head of the page.
        /// </summary>
        public List<ElementComponentRecord> Components { get; set; }
    }
}
