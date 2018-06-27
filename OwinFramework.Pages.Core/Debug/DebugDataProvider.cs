using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information for a data provider
    /// </summary>
    public class DebugDataProvider: DebugInfo
    {
        /// <summary>
        /// The package that this provider is in
        /// </summary>
        public DebugPackage Package { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugDataProvider()
        {
            Type = "Data provider";
        }
    }
}
