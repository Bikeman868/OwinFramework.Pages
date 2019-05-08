using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    public class LayoutZoneRecord
    {
        /// <summary>
        /// The name of the zone within the layout to configure
        /// </summary>
        [Mapping("zone")]
        public string ZoneName { get; set; }

        /// <summary>
        /// The unique ID of the region to render into this region. 
        /// </summary>
        public long? RegionId { get; set; }

        /// <summary>
        /// The unique ID of the layout to render into this region. 
        /// </summary>
        public long? LayoutId { get; set; }

        /// <summary>
        /// Can be 'zone', 'Layout', 'Component', 'Html' or 'Template'
        /// </summary>
        [Mapping("contentType")]
        public string ContentType { get; set; }

        /// <summary>
        /// The name of the thing to put into this region. The ContentType
        /// specifies how to interpret this name. When the ContentType is 'Html'
        /// this is the name of a localizable asset.
        /// </summary>
        [Mapping("contentName")]
        public string ContentName { get; set; }

        /// <summary>
        /// The value to put into this region. Only applies when ContentType
        /// is 'Html'
        /// </summary>
        [Mapping("contentValue")]
        public string ContentValue { get; set; }
    }
}
