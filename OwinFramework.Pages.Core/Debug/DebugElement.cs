namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about an element
    /// </summary>
    public class DebugElement: DebugInfo
    {
        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugElement()
        {
            Type = "Element";
        }

        /// <summary>
        /// Returns a default description
        /// </summary>
        public override string ToString()
        {
            return Type + " '" + (Name ?? string.Empty) + "'";
        }
    }
}
