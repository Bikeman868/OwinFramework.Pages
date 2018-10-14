using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// The IoC dependencies are wrapped in this factory so that when
    /// new dependencies are added it does not change the constructor
    /// which would break any application code that inherits from it
    /// </summary>
    public interface IComponentDependenciesFactory
    {
        /// <summary>
        /// Returns a factory that can construct data consumer mixins
        /// </summary>
        IDataConsumerFactory DataConsumerFactory { get; }

        /// <summary>
        /// Returns a factory that can construct css writers
        /// </summary>
        ICssWriterFactory CssWriterFactory { get; }

        /// <summary>
        /// Returns a factory that can construct css writers
        /// </summary>
        IJavascriptWriterFactory JavascriptWriterFactory { get; }

        /// <summary>
        /// Returns a singleton that manages localized assets
        /// </summary>
        IAssetManager AssetManager { get; }

        /// <summary>
        /// Returns a singleton that manages name resolution within packages
        /// </summary>
        INameManager NameManager { get; }

        /// <summary>
        /// Constructs and initializes a component dependencies instance
        /// specific to the request
        /// </summary>
        IComponentDependencies Create(IOwinContext context);
    }
}
