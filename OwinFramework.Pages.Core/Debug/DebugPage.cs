using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Contains debugging information about a page
    /// </summary>
    public class DebugPage : DebugInfo
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
        /// The data scope provider associated with this page
        /// </summary>
        public DebugDataScopeProvider Scope { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugPage()
        {
            Type = "Page";
        }
    }
}
