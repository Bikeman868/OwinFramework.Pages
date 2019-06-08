using System;
using System.Reflection;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// This can be used to build elements using a fluent syntax
    /// </summary>
    public interface IFluentBuilder: 
        IComponentBuilder,
        IRegionBuilder,
        ILayoutBuilder,
        IPageBuilder,
        IServiceBuilder,
        IModuleBuilder,
        IPackageBuilder,
        IDataProviderBuilder
    {
        /// <summary>
        /// Registers all components, layouts, regions etc defined in the package. Optionally
        /// allows you to modify the namespace of the package from the default one
        /// </summary>
        /// <param name="package">The package instance to register</param>
        /// <param name="namespaceName">The namespace to use for all css class
        /// names and javascript functions within this package</param>
        /// <param name="factory">If this package includes services that you must
        /// supply a factory method here. It will only be used to construct
        /// instances of services</param>
        IPackage RegisterPackage(IPackage package, string namespaceName, Func<Type, object> factory = null);

        /// <summary>
        /// Registers all components, layouts, regions etc defined in the package using
        /// the default namespace for the package
        /// </summary>
        /// <param name="package">The package instance to register</param>
        /// <param name="factory">If this package includes services that you must
        /// supply a factory method here. It will only be used to construct
        /// instances of services</param>
        IPackage RegisterPackage(IPackage package, Func<Type, object> factory);

        /// <summary>
        /// Searches within the given assembly for all eleemnts and
        /// registers them.
        /// </summary>
        /// <param name="assembly">The assembly to search for eleemnts</param>
        /// <param name="factory">Optional factory. If supplied then it will be used
        /// to construct classes that implement interfaces like ILayout, IRegion etc.</param>
        void Register(Assembly assembly, Func<Type, object> factory = null);

        /// <summary>
        /// Registeres a class as an element if it is one, does nothing
        /// if this is not an element
        /// </summary>
        /// <param name="type">The element type to register</param>
        /// <param name="factory">Optional factory. If supplied then it will be used
        /// to construct classes that implement interfaces like ILayout, IRegion etc.</param>
        object Register(Type type, Func<Type, object> factory = null);

        /// <summary>
        /// Registeres a an element that was already constructed by the application
        /// </summary>
        /// <param name="element">The element to register</param>
        void Register(object element);

        /// <summary>
        /// Defines how packages are built
        /// </summary>
        IPackageBuilder PackageBuilder { get; set; }

        /// <summary>
        /// Defines how modules are built
        /// </summary>
        IModuleBuilder ModuleBuilder { get; set; }

        /// <summary>
        /// Defines how pages are built
        /// </summary>
        IPageBuilder PageBuilder { get; set; }

        /// <summary>
        /// Defines how layouts are built
        /// </summary>
        ILayoutBuilder LayoutBuilder { get; set; }

        /// <summary>
        /// Defines how regions are built
        /// </summary>
        IRegionBuilder RegionBuilder { get; set; }

        /// <summary>
        /// Defines how components are built
        /// </summary>
        IComponentBuilder ComponentBuilder { get; set; }

        /// <summary>
        /// Defines how services are built
        /// </summary>
        IServiceBuilder ServiceBuilder { get; set; }

        /// <summary>
        /// Defines how services are built
        /// </summary>
        IDataProviderBuilder DataProviderBuilder { get; set; }
    }
}
