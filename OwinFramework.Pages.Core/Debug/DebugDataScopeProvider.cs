﻿using System.Collections.Generic;

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
        /// A list of scopes introduced
        /// </summary>
        public List<DebugDataScope> Scopes { get; set; }

        /// <summary>
        /// A list of data supplies that will be added to the data context
        /// </summary>
        public List<DebugSuppliedDependency> DataSupplies { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugDataScopeProvider()
        {
            Type = "Data scope";
        }

        /// <summary>
        /// Returns a default description
        /// </summary>
        public override string ToString()
        {
            return base.ToString() + " #" + Id;
        }
    }
}
