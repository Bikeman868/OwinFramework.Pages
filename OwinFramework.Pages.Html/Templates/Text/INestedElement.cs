namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// This is implemented by document elements that have a nesting level
    /// such as headings
    /// </summary>
    public interface INestedElement
    {
        /// <summary>
        /// The nesting level of this element. For example with headings
        /// an <h1/> tag has a level of 1 and <h2/> is level 2 etc.
        /// </summary>
        int Level { get; set; }
    }
}