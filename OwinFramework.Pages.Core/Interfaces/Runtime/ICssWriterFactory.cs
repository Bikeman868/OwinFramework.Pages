using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// A factory for constructing HtmlWriter instancces
    /// </summary>
    public interface ICssWriterFactory
    {
        /// <summary>
        /// Creates and initializes an instance that can write CSS using the configuration
        /// of a render context
        /// </summary>
        ICssWriter Create(IRenderContext context);

        /// <summary>
        /// Creates and initializes an instance that can write CSS using
        /// the default configuration
        /// </summary>
        ICssWriter Create();
    }
}
