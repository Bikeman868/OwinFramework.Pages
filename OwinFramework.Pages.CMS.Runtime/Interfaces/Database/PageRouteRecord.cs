using Prius.Contracts.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    public class PageRouteRecord
    {
        /// <summary>
        /// The page version that this route applies to
        /// </summary>
        [Mapping("pageVersionId")]
        public long PageVersionId { get; set; }

        /// <summary>
        /// The URL path to this page. Can include wildcard characters
        /// to match many routes, but this is only useful with hand written
        /// components that display different content according to the
        /// URL path of the page.
        /// </summary>
        [Mapping("path")]
        public string Path { get; set; }

        /// <summary>
        /// Pages are compared to the incomming request in highest to lowest
        /// priority order. Routes that contain wildcards should generally have
        /// negative priorities so that fully specified URLs can override them.
        /// </summary>
        [Mapping("priority")]
        public int Priority { get; set; }
    }
}
