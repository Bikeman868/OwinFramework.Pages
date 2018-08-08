using System;

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
        /// The type of data to repeat inside the region
        /// </summary>
        public Type RepeatType { get; set; }

        /// <summary>
        /// The type of list to get from context for repeating
        /// </summary>
        public Type ListType { get; set; }

        /// <summary>
        /// The scope name to use for the repeated type
        /// </summary>
        public string RepeatScope { get; set; }

        /// <summary>
        /// The scope name to use for retrieving the list
        /// </summary>
        public string ListScope { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugRegion()
        {
            Type = "Region";
        }

        /// <summary>
        /// Returns a default description
        /// </summary>
        public override string ToString()
        {
            if (ListType != null)
                return "repeating " + base.ToString();

            return base.ToString();
        }
    }
}
