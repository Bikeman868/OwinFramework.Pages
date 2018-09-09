namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information for a data provider
    /// </summary>
    public class DebugPackage: DebugInfo
    {
        /// <summary>
        /// The module for this package
        /// </summary>
        public DebugModule Module { get; set; }

        /// <summary>
        /// The namespace of this package
        /// </summary>
        public string NamespaceName { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugPackage()
        {
            Type = "Package";
        }

        /// <summary>
        /// Indicates of this debug info is worth displaying
        /// </summary>
        public override bool HasData()
        {
            return !string.IsNullOrEmpty(NamespaceName);
        }
    }
}
