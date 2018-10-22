namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// This interface is implemented by document elements that cause a 
    /// break in the flow of the document
    /// </summary>
    public interface IBreakElement
    {
        /// <summary>
        /// The type of break
        /// </summary>
        BreakTypes BreakType { get; set; }
    }
}