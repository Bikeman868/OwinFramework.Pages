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
        IServiceBuilder
    {
        /// <summary>
        /// Registers all components, layouts, regions etc defined in the package
        /// </summary>
        void Register(IPackage package);

        /// <summary>
        /// Searches within the given assembly for all eleemnts and
        /// registers them.
        /// </summary>
        /// <param name="assembly">The assembly to search for eleemnts</param>
        void Register(Assembly assembly);

        /// <summary>
        /// Registeres a class as an element if it is one, does nothing
        /// if this is not an element
        /// </summary>
        /// <param name="type">The element type to register</param>
        void Register(Type type);

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
    }
}
