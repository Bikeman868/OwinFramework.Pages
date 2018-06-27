using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Base class for used to gather debugging information
    /// </summary>
    public class DebugInfo
    {
        /// <summary>
        /// The name of the component
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of component
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The live instance that this is debug info for
        /// </summary>
        public object Instance { get; set; }
    }
}
