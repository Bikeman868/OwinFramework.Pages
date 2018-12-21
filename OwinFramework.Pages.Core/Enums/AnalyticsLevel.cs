namespace OwinFramework.Pages.Core.Enums
{
    /// <summary>
    /// Defines how detailed the analytics should be
    /// </summary>
    public enum AnalyticsLevel
    {
        /// <summary>
        /// Disables analytics recording
        /// </summary>
        None,

        /// <summary>
        /// Captures basic stats like average request time and average request rate
        /// </summary>
        Basic,

        /// <summary>
        /// Turns on all available analytics
        /// </summary>
        Full
    }
}
