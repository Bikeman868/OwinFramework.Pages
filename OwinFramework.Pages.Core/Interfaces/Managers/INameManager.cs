using System;

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
        /// Registers the name of a component
        /// </summary>
        /// <param name="element">The component to register</param>
        void Register(IElement element);

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
        /// <returns>A region or null if not found</returns>
        IRegion ResolveRegion(string name, IPackage package = null);

        /// <summary>
        /// Finds the layout with the specified name
        /// </summary>
        /// <param name="name">The name of the layout to find. The name can be qualified with a 
        /// namespace and a colon in front to specify the package. If no namespace is specified
        /// then the specified package will be searched first followed by the global namespace</param>
        /// <param name="package">Optional package, makes name resolution use this namespace
        /// first if no namespace is in the name</param>
        /// <returns>A layout or null if not found</returns>
        ILayout ResolveLayout(string name, IPackage package = null);

        /// <summary>
        /// Finds the page with the specified name
        /// </summary>
        /// <param name="name">The name of the page to find. The name can be qualified with a 
        /// namespace and a colon in front to specify the package. If no namespace is specified
        /// then the specified package will be searched first followed by the global namespace</param>
        /// <param name="package">Optional package, makes name resolution use this namespace
        /// first if no namespace is in the name</param>
        /// <returns>A layout or null if not found</returns>
        IPage ResolvePage(string name, IPackage package = null);

        /// <summary>
        /// Finds the service with the specified name
        /// </summary>
        /// <param name="name">The name of the service to find. The name can be qualified with a 
        /// namespace and a colon in front to specify the package. If no namespace is specified
        /// then the specified package will be searched first followed by the global namespace</param>
        /// <param name="package">Optional package, makes name resolution use this namespace
        /// first if no namespace is in the name</param>
        /// <returns>A layout or null if not found</returns>
        IService ResolveService(string name, IPackage package = null);

        /// <summary>
        /// Finds the module with the specified name
        /// </summary>
        /// <param name="name">The name of the module to find</param>
        /// <returns>A module or null if not found</returns>
        IModule ResolveModule(string name);

        /// <summary>
        /// Finds the package with the specified name
        /// </summary>
        /// <param name="name">The name of the package to find</param>
        /// <returns>A package or null if not found</returns>
        IPackage ResolvePackage(string name);

        /// <summary>
        /// Makes up a random name that is unique and in the same namespace as the element. The
        /// generated name can be used as a JavaScript identifier or a css class name.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>A randomly generated short name</returns>
        string MakeShortName(IElement element);
    }
}
