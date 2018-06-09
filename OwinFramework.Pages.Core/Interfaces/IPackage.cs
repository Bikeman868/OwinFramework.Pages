using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A package is a collection of pages, components, layouts
    /// and services that can be used to build websites. Packages
    /// are discovered at startup which makes all of their
    /// elements available to the website builder
    /// </summary>
    public interface IPackage
    {
        /// <summary>
        /// The unique name of this package
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The unique namespace for everything in this package
        /// </summary>
        string NamespaceName { get; set; }

        /// <summary>
        /// Gets and sets the module for this package. Setting the module
        /// sets the default deployment for all assets in this package
        /// </summary>
        IModule Module { get; set; }

        /// <summary>
        /// This method is called to give your package a chance to build
        /// all of the elements in the package.
        /// </summary>
        IPackage Build(IFluentBuilder builder);
    }
}
