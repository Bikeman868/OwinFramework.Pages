using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about a layout
    /// </summary>
    public class DebugRenderContext : DebugInfo
    {
        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugRenderContext()
        {
            Type = "Render context";
        }
    }
}
