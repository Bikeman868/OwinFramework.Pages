namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// A factory for constructing HtmlWriter instancces
    /// </summary>
    public interface IHtmlWriterFactory
    {
        /// <summary>
        /// Creates and initializes an instance that can write Html
        /// </summary>
        /// <param name="indented">Choose readable vs compact</param>
        /// <returns></returns>
        IHtmlWriter Create(bool indented = true);
    }
}
