using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Collections;
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
    public interface IPageDependenciesFactory
    {
        /// <summary>
        /// Constructs and initializes a page dependencies instance
        /// specific to the request
        /// </summary>
        IPageDependencies Create(IOwinContext context, Action<IOwinContext, Func<string>> trace);

        /// <summary>
        /// The name manager is a singleton and therefore alwaya available
        /// </summary>
        INameManager NameManager { get; }

        /// <summary>
        /// The asset manager is a singleton and therefore alwaya available
        /// </summary>
        IAssetManager AssetManager { get; }

        /// <summary>
        /// Factory for constructing CSS writers
        /// </summary>
        ICssWriterFactory CssWriterFactory { get; }

        /// <summary>
        /// Factory for constructing Javascript writers
        /// </summary>
        IJavascriptWriterFactory JavascriptWriterFactory { get; }

        /// <summary>
        /// A factory for constructing data scope providers
        /// </summary>
        IDataScopeProviderFactory DataScopeProviderFactory { get; }

        /// <summary>
        /// A factory for constructing data consumers
        /// </summary>
        IDataConsumerFactory DataConsumerFactory { get; }

        /// <summary>
        /// A factory for constructing thread-safe dictionaries
        /// </summary>
        IDictionaryFactory DictionaryFactory { get; }

        /// <summary>
        /// A factory for constructing data contexts
        /// </summary>
        IDataContextFactory DataContextFactory { get; }
    }
}
