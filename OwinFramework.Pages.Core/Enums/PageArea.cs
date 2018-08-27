namespace OwinFramework.Pages.Core.Enums
{
    /// <summary>
    /// Defines the parts of the page that can be written to
    /// </summary>
    public enum PageArea
    {
        /// <summary>
        /// The head section of the page
        /// </summary>
        Head,

        /// <summary>
        /// The page title within the head
        /// </summary>
        Title,

        /// <summary>
        /// The CSS styles defined within the head section
        /// </summary>
        Styles,

        /// <summary>
        /// The JavaScript functions defined within the head section
        /// </summary>
        Scripts,

        /// <summary>
        /// The main body of the HTML document
        /// </summary>
        Body,

        /// <summary>
        /// The initialization JavaScript that goes at the end of the body
        /// </summary>
        Initialization,

        /// <summary>
        /// This must be the last value in this enumeration
        /// </summary>
        MaxValue
    }
}
