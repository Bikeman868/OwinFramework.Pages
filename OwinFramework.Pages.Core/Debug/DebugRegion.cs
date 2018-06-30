using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about a layout
    /// </summary>
    public class DebugRegion : DebugElement
    {
        /// <summary>
        /// The layout's regions
        /// </summary>
        public DebugInfo Content { get; set; }

        /// <summary>
        /// If this is a page specific instance then this contains debug info
        /// for the region definition that this is an instance of
        /// </summary>
        public DebugRegion InstanceOf { get; set; }

        /// <summary>
        /// The data scope provider associated with this region
        /// </summary>
        public DebugDataScopeProvider Scope { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugRegion()
        {
            Type = "Region";
        }
    }
}
