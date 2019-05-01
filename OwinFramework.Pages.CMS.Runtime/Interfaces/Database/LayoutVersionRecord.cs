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
        /// The name of a permission that is required to view this page
        /// </summary>
        [Mapping("regionNesting")]
        public string RegionNesting { get; set; }

        /// <summary>
        /// Overrides the content of the page layout just for this page
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
