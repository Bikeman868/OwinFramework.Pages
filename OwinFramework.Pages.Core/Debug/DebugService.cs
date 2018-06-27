using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Contains debugging information about a page
    /// </summary>
    public class DebugService : DebugInfo
    {
        /// <summary>
        /// The layout of this page
        /// </summary>
        public DebugLayout Layout { get; set; }

        /// <summary>
        /// Debug information about the URLs that are routed to this page
        /// </summary>
        public List<DebugRoute> Routes { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugService()
        {
            Type = "Service";
        }
    }
}
