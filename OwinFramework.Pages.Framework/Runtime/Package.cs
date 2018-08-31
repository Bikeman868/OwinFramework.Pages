using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;

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
        /// The element type is Package
        /// </summary>
        public ElementType ElementType { get { return ElementType.Package; } }

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

        /// <summary>
        /// IoC dependencies are wrapped up into a single interface
        /// so that the constructor signature will not change if more
        /// dependencies are added later
        /// </summary>
        protected readonly IPackageDependenciesFactory Dependencies;

        /// <summary>
        /// Default IoC constructor
        /// </summary>
        public Package(IPackageDependenciesFactory dependencies)
        {
            // DO NOT CHANGE THE METHOD SIGNATURE OF THIS CONSTRUCTOR
           
            Dependencies = dependencies;
        }

        /// <summary>
        /// Override this to build package elements
        /// </summary>
        /// <param name="fluentBuilder">A fluent builder that has a package context and
        /// builds everything within this package</param>
        public virtual IPackage Build(IFluentBuilder fluentBuilder)
        {
            return this;
        }

        /// <summary>
        /// Returns debugging information
        /// </summary>
        public DebugPackage GetDebugInfo()
        {
            return new DebugPackage
            {
                Name = Name,
                Instance = this,
                Module = Module.GetDebugInfo<DebugModule>()
            };
        }
    }
}
