using System;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Managers
{
    /// <summary>
    /// The name manager keeps track of what everything is called and provides name
    /// resolution. It also manages the namespaces associated with modules, and
    /// provides a name shortening service.
    /// </summary>
    public interface INameManager
    {
        /// <summary>
        /// Call this method once all of the names have been registered to
        /// resolve name references between elements
        /// </summary>
        void Bind();

        /// <summary>
        /// Registers the name of an element
        /// </summary>
        /// <param name="element">The element to register</param>
        void Register(IElement element);

        /// <summary>
        /// Registers the name of a runable
        /// </summary>
        /// <param name="runable">The runable to register</param>
        void Register(IRunable runable);

        /// <summary>
        /// Registers the name of a module
        /// </summary>
        /// <param name="module">The module to register</param>
        void Register(IModule module);

        /// <summary>
        /// Registers the name of a package
        /// </summary>
        /// <param name="package">The package to register</param>
        void Register(IPackage package);

        /// <summary>
        /// Registers the name of a data provider
        /// </summary>
        /// <param name="dataProvider">The data provider to register</param>
        void Register(IDataProvider dataProvider);

        /// <summary>
        /// Adds a callback function to execute after all components have been
        /// registered. This can be used to resolve name references between elements
        /// </summary>
        /// <param name="resolutionAction">A callback function to call when all
        /// names are defined</param>
        void AddResolutionHandler(Action resolutionAction);

        /// <summary>
        /// Adds a callback function to execute after all components have been
        /// registered. This can be used to resolve name references between elements
        /// </summary>
        /// <param name="resolutionAction">A callback function to call when all
        /// names are defined</param>
        void AddResolutionHandler(Action<INameManager> resolutionAction);

        /// <summary>
        /// Adds a callback function to execute after all components have been
        /// registered. This can be used to resolve name references between elements
        /// </summary>
        /// <param name="resolutionAction">A callback function to call when all</param>
        /// <param name="context">Contextual data to pass back to the handler
        /// names are defined</param>
        void AddResolutionHandler<T>(Action<INameManager, T> resolutionAction, T context);

        /// <summary>
        /// Finds the component with the specified name
        /// </summary>
        /// <param name="name">The name of the component to find. The name can be qualified with a 
        /// namespace and a colon in front to specify the package. If no namespace is specified
        /// then the specified package will be searched first followed by the global namespace</param>
        /// <param name="package">Optional package, makes name resolution use this namespace
        /// first if no namespace is in the name</param>
        /// <returns>A component or null if not found</returns>
        IComponent ResolveComponent(string name, IPackage package = null);

        /// <summary>
        /// Finds the region with the specified name
        /// </summary>
        /// <param name="name">The name of the region to find. The name can be qualified with a 
        /// namespace and a colon in front to specify the package. If no namespace is specified
        /// then the specified package will be searched first followed by the global namespace</param>
        /// <param name="package">Optional package, makes name resolution use this namespace
        /// first if no namespace is in the name</param>
        IRegion ResolveRegion(string name, IPackage package = null);

        /// <summary>
        /// Finds the layout with the specified name
        /// </summary>
        /// <param name="name">The name of the layout to find. The name can be qualified with a 
        /// namespace and a colon in front to specify the package. If no namespace is specified
        /// then the specified package will be searched first followed by the global namespace</param>
        /// <param name="package">Optional package, makes name resolution use this namespace
        /// first if no namespace is in the name</param>
        ILayout ResolveLayout(string name, IPackage package = null);

        /// <summary>
        /// Finds the page with the specified name
        /// </summary>
        /// <param name="name">The name of the page to find. The name can be qualified with a 
        /// namespace and a colon in front to specify the package. If no namespace is specified
        /// then the specified package will be searched first followed by the global namespace</param>
        /// <param name="package">Optional package, makes name resolution use this namespace
        /// first if no namespace is in the name</param>
        IPage ResolvePage(string name, IPackage package = null);

        /// <summary>
        /// Finds the service with the specified name
        /// </summary>
        /// <param name="name">The name of the service to find. The name can be qualified with a 
        /// namespace and a colon in front to specify the package. If no namespace is specified
        /// then the specified package will be searched first followed by the global namespace</param>
        /// <param name="package">Optional package, makes name resolution use this namespace
        /// first if no namespace is in the name</param>
        IService ResolveService(string name, IPackage package = null);

        /// <summary>
        /// Finds the data provider with the specified name
        /// </summary>
        /// <param name="name">The name of the data provider to find. The name can be qualified with a 
        /// namespace and a colon in front to specify the package. If no namespace is specified
        /// then the specified package will be searched first followed by the global namespace</param>
        /// <param name="package">Optional package, makes name resolution use this namespace
        /// first if no namespace is in the name</param>
        IDataProvider ResolveDataProvider(string name, IPackage package = null);

        /// <summary>
        /// Finds the module with the specified name
        /// </summary>
        /// <param name="name">The name of the module to find</param>
        IModule ResolveModule(string name);

        /// <summary>
        /// Finds the package with the specified name
        /// </summary>
        /// <param name="name">The name of the package to find</param>
        IPackage ResolvePackage(string name);

        /// <summary>
        /// Makes up a random namespace qualified name that is unique and in the same 
        /// namespace as the element. The generated name can be used as a JavaScript 
        /// identifier or a css class name.
        /// </summary>
        string GenerateAssetName(IElement element);

        /// <summary>
        /// Allocates an asset name if the asset has no name already
        /// </summary>
        void EnsureAssetName(IElement element, ref string assetName);
    }
}
