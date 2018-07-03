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
        /// The dependency that is being fulfilled by this provider
        /// </summary>
        public string Dependency { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugDataProvider()
        {
            Type = "Data provider";
        }
    }
}
