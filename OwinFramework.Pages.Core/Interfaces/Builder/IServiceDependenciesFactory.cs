using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// The IoC dependencies are wrapped in this factory so that when
    /// new dependencies are added it does not change the constructor
    /// which would break any application code that inherits from it
    /// </summary>
    public interface IServiceDependenciesFactory
    {
        /// <summary>
        /// Constructs and initializes a page dependencies instance
        /// specific to the request
        /// </summary>
        IServiceDependencies Create(IOwinContext context);

        /// <summary>
        /// The name manager is a singleton and therefore alwaya available
        /// </summary>
        INameManager NameManager { get; }

        /// <summary>
        /// The asset manager is a singleton and therefore alwaya available
        /// </summary>
        IAssetManager AssetManager { get; }

        /// <summary>
        /// A factory for constructing data consumers
        /// </summary>
        IDataConsumerFactory DataConsumerFactory { get; }
    }
}
