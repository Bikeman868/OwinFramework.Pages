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
            if (Children == null || Children.Count == 0)
                return base.ToString();

            return  Children.Aggregate("layout with", (s, r) => s + " '" + r.Name + "'") + " regions";
        }
    }
}
