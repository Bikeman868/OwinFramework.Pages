using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A package is a collection of pages, components, layouts
    /// and services that can be used to build websites. Packages
    /// are discovered at startup which makes all of their
    /// elements available to the website builder
    /// </summary>
    public interface IPackage: INamed
    {
        /// <summary>
        /// The unique namespace for everything in this package
        /// </summary>
        string NamespaceName { get; set; }

        /// <summary>
        /// Gets debugging information from this package
        /// </summary>
        DebugPackage GetDebugInfo();

        /// <summary>
        /// Gets and sets the module for this package. Setting the module
        /// sets the default deployment for all assets in this package
        /// </summary>
        IModule Module { get; set; }

        /// <summary>
        /// This method is called to give your package a chance to build
        /// all of the elements in the package.
        /// </summary>
        IPackage Build(IFluentBuilder fluentBuilder);
    }
}
