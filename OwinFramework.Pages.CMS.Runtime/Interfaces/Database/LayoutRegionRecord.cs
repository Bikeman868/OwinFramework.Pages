using Prius.Contracts.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    public class LayoutRegionRecord
    {
        /// <summary>
        /// The name of the region within the layout to configure
        /// </summary>
        [Mapping("region")]
        public string RegionName { get; set; }

        /// <summary>
        /// Can be 'Layout', 'Component', 'Html' or 'Template'
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
