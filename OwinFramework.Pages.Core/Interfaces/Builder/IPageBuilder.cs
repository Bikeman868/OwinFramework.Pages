namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the page builder to construct pages using a fluent syntax
    /// </summary>
    public interface IPageBuilder
    {
        /// <summary>
        /// Starts building a new page
        /// </summary>
        IPageDefinition Page();
    }
}
