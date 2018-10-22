namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// When documents contain references to other documents, this specifies
    /// how those references should be handled
    /// </summary>
    public enum LinkTypes 
    { 
        /// <summary>
        /// The link is a reference to another document. The user should be
        /// acle to click the link to navigate to the linked document
        /// </summary>
        Reference, 

        /// <summary>
        /// The link is a reference to an image that should be embedded into
        /// this document inline.
        /// </summary>
        Image,

        /// <summary>
        /// The link is a reference to a page that should be embedded into
        /// the document
        /// </summary>
        Iframe
    }
}