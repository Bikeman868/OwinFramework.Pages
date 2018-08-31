using System.Collections.Generic;
using System.Linq;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about a layout
    /// </summary>
    public class DebugLayout : DebugInfo
    {
        /// <summary>
        /// The layout's regions
        /// </summary>
        public List<DebugLayoutRegion> Regions { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugLayout()
        {
            Type = "Layout";
        }

        /// <summary>
        /// Returns the default description
        /// </summary>
        public override string ToString()
        {
            if (Regions == null || Regions.Count == 0)
                return base.ToString();

            return  Regions.Aggregate("layout with", (s, r) => s + " '" + r.Name + "'") + " regions";
        }
    }
}
