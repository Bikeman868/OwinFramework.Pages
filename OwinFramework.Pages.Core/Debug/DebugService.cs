using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Contains debugging information about a page
    /// </summary>
    public class DebugService : DebugInfo
    {
        /// <summary>
        /// Debug information about the URLs that are routed to this page
        /// </summary>
        public List<DebugRoute> Routes { get; set; }

        /// <summary>
        /// The data scope provider associated with this service
        /// </summary>
        public DebugDataScopeRules Scope { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugService()
        {
            Type = "Service";
        }
    }
}
