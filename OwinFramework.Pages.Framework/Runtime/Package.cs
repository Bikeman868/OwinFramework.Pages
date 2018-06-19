using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Framework.Runtime
{
    /// <summary>
    /// You can choose to inherit from this base class if you are implementing
    /// the IPackage interface, this will insulate your class from any changes
    /// to the IPackage interface in the future
    /// </summary>
    public class Package: IPackage
    {
        /// <summary>
        /// The unique name of this package
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The unique name of this package
        /// </summary>
        public string NamespaceName { get; set; }

        /// <summary>
        /// Assigns a package to a module defining the default deployment scheme 
        /// for everything in this package
        /// </summary>
        public IModule Module { get; set; }

        public Package(IPackageDependenciesFactory dependencies)
        { }

        /// <summary>
        /// Override this to build package elements
        /// </summary>
        /// <param name="fluentBuilder">A fluent builder that has a package context and
        /// builds everything within this package</param>
        public virtual IPackage Build(IFluentBuilder fluentBuilder)
        {
            return this;
        }
    }
}
