using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Contains debugging information about a data scope
    /// </summary>
    public class DebugDataScopeProvider : DebugInfo
    {
        /// <summary>
        /// The unique ID of this data scope provider
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The parent provider
        /// </summary>
        public DebugDataScopeProvider Parent { get; set; }

        /// <summary>
        /// The children of this provider
        /// </summary>
        public List<DebugDataScopeProvider> Children { get; set; }

        /// <summary>
        /// A list of scopes introduced
        /// </summary>
        public List<string> Scopes { get; set; }

        /// <summary>
        /// A list of dependencies that resolved to this scope
        /// </summary>
        public List<string> Dependencies { get; set; }

        /// <summary>
        /// The data providers that will populate the data context of requests
        /// </summary>
        public List<DebugDataProvider> DataProviders { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugDataScopeProvider()
        {
            Type = "Data scope";
        }
    }
}
