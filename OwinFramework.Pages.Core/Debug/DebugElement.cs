using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about an element
    /// </summary>
    public class DebugElement: DebugInfo
    {
        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugElement()
        {
            Type = "Element";
        }
    }
}
