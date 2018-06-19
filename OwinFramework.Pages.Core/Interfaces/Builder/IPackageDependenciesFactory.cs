using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// The IoC dependencies are wrapped in this factory so that when
    /// new dependencies are added it does not change the constructor
    /// which would break any application code that inherits from it
    /// </summary>
    public interface IPackageDependenciesFactory
    {
        /// <summary>
        /// Constructs and initializes a package dependencies instance
        /// specific to the request
        /// </summary>
        IPackageDependencies Create(IOwinContext context);

        /// <summary>
        /// Name manager
        /// </summary>
        INameManager NameManager { get; }

        /// <summary>
        /// Asset maneger
        /// </summary>
        IAssetManager AssetManager { get; }

        /// <summary>
        /// Factory for constructing module dependencies
        /// </summary>
        IModuleDependenciesFactory ModuleDependenciesFactory { get; }

        /// <summary>
        /// Factory for constructing page dependencies
        /// </summary>
        IPageDependenciesFactory PageDependenciesFactory { get; }

        /// <summary>
        /// Factory for constructing layout dependencies
        /// </summary>
        ILayoutDependenciesFactory LayoutDependenciesFactory { get; }

        /// <summary>
        /// Factory for constructing region dependencies
        /// </summary>
        IRegionDependenciesFactory RegionDependenciesFactory { get; }

        /// <summary>
        /// Factory for constructing component dependencies
        /// </summary>
        IComponentDependenciesFactory ComponentDependenciesFactory { get; }
    }
}
