using OwinFramework.Pages.Core.Enums;

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
        IHtmlWriter Create(IFrameworkConfiguration frameworkConfiguration);
    }
}
