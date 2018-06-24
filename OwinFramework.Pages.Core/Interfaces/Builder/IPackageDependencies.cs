using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the injected dependencies of Package. They are packaged like this
    /// to avoid changing the constructor signature when new dependencies are added.
    /// </summary>
    public interface IPackageDependencies : IDisposable
    {
        /// <summary>
        /// The request rendering context
        /// </summary>
        IRenderContext RenderContext { get; }

        /// <summary>
        /// Asset manager
        /// </summary>
        IAssetManager AssetManager { get; }

        /// <summary>
        /// Name manager
        /// </summary>
        INameManager NameManager { get; }

        /// <summary>
        /// Dependencies to pass the module constructors
        /// </summary>
        IModuleDependenciesFactory ModuleDependenciesFactory { get; }

        /// <summary>
        /// Dependencies to pass the page constructors
        /// </summary>
        IPageDependenciesFactory PageDependenciesFactory { get; }

        /// <summary>
        /// Dependencies to pass the layout constructors
        /// </summary>
        ILayoutDependenciesFactory LayoutDependenciesFactory { get; }

        /// <summary>
        /// Dependencies to pass the region constructors
        /// </summary>
        IRegionDependenciesFactory RegionDependenciesFactory { get; }

        /// <summary>
        /// Dependencies to pass the component constructors
        /// </summary>
        IComponentDependenciesFactory ComponentDependenciesFactory { get; }

        /// <summary>
        /// Initializes this instance for a specific request
        /// </summary>
        IPackageDependencies Initialize(IOwinContext context);
    }
}
