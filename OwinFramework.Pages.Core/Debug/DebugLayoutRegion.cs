namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about a layout
    /// </summary>
    public class DebugLayoutRegion: DebugInfo
    {
        /// <summary>
        /// The regions instance
        /// </summary>
        public DebugRegion Region { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugLayoutRegion()
        {
            Type = "Layout region";
        }

        /// <summary>
        /// Indicates of this debug info is worth displaying
        /// </summary>
        public override bool HasData()
        {
            return Region != null && Region.HasData();
        }
    }
}
