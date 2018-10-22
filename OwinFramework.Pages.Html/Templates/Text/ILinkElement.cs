namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// This interface is implemented by document elements that contain the
    /// URL of another resource. These are typically links to other 
    /// </summary>
    public interface ILinkElement
    {
        /// <summary>
        /// The type of link
        /// </summary>
        LinkTypes LinkType { get; set; }

        /// <summary>
        /// The URL of the linked resource
        /// </summary>
        string LinkAddress { get; set; }

        /// <summary>
        /// Alternate text to show when resource is loading
        /// Also used by readers for blind people
        /// </summary>
        string AltText { get; set; }
    }
}