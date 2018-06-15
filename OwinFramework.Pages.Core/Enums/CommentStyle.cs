namespace OwinFramework.Pages.Core.Enums
{
    /// <summary>
    /// Defines how the HtmlWriter will write comments
    /// </summary>
    public enum CommentStyle
    {
        /// <summary>
        /// Writes comments in an XML syntax - also works with Html
        /// </summary>
        Xml,

        /// <summary>
        /// Writes two forward slashes at the start of the line
        /// </summary>
        SingleLineC,

        /// <summary>
        /// Encloses the comment in a /* */ pair
        /// </summary>
        MultiLineC
    }
}
