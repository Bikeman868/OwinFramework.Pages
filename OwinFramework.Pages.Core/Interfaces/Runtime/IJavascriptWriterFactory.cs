using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// A factory for constructing HtmlWriter instancces
    /// </summary>
    public interface IJavascriptWriterFactory
    {
        /// <summary>
        /// Creates and initializes an instance that can write Javascript
        /// </summary>
        IJavascriptWriter Create(IFrameworkConfiguration frameworkConfiguration);

        /// <summary>
        /// Creates and initializes an instance that can write Javascript
        /// </summary>
        IJavascriptWriter Create(IRenderContext context);
    }
}
