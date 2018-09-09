namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug info for routes
    /// </summary>
    public class DebugRoute: DebugInfo
    {
        /// <summary>
        /// The URLs that match this route
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugRoute()
        {
            Type = "Route";
        }

        /// <summary>
        /// Indicates of this debug info is worth displaying
        /// </summary>
        public override bool HasData()
        {
            return !string.IsNullOrEmpty(Route);
        }
    }
}
