using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about a layout
    /// </summary>
    public class DebugDataContext : DebugInfo
    {
        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugDataContext()
        {
            Type = "Data context";
        }

        /// <summary>
        /// The scope provider that will be used to resolve
        /// requests for data that is missing from the data context
        /// </summary>
        public IDataScopeProvider Scope { get; set; }

        /// <summary>
        /// The properties that are overriden in this context
        /// </summary>
        public List<Type> Properties { get; set; }
    }
}
