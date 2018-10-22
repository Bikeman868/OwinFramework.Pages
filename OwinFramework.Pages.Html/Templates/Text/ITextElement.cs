namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// This is implemented by document elements that contain plain text
    /// </summary>
    public interface ITextElement
    {
        /// <summary>
        /// The text of this element
        /// </summary>
        string Text { get; set; }
    }
}