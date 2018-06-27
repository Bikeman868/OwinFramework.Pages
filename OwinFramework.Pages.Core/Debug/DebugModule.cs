using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information for a data provider
    /// </summary>
    public class DebugModule: DebugInfo
    {
        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugModule()
        {
            Type = "Module";
        }
    }
}
