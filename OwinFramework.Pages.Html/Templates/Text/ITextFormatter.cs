namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// This is a stateless singleton that provides utility methods for
    /// formatting strings
    /// </summary>
    public interface ITextFormatter
    {
        /// <summary>
        /// Truncates long strings adding the specified padding to the end of the 
        /// truncated string if it was truncated. Can truncate at word boundaries 
        /// or at specified length
        /// </summary>
        string LimitLength(string text, int maxLength, string padString, bool wholeWords);

        /// <summary>
        /// Truncates long strings adding the specified padding to the end of the 
        /// truncated string if it was truncated. Truncates at word boundaries where possible.
        /// </summary>
        string LimitLength(string text, int maxLength, string padString);

        /// <summary>
        /// Truncates long strings adding '...' at the end of the string if
        /// it was truncated. Truncates at word boundaries where possible.
        /// </summary>
        string LimitLength(string text, int maxLength);
    }
}
