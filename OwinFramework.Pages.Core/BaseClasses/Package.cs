using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Core.BaseClasses
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

        /// <summary>
        /// Builds the components in this package
        /// </summary>
        public virtual void BuildComponents(IComponentBuilder componentBuilder)
        {
        }

        /// <summary>
        /// Builds the regions in this package
        /// </summary>
        public virtual void BuildRegions(IRegionBuilder regionBuilder)
        {
        }

        /// <summary>
        /// Builds the layouts in this package
        /// </summary>
        public virtual void BuildLayouts(ILayoutBuilder layoutBuilder)
        {
        }

        /// <summary>
        /// Builds the services in this package
        /// </summary>
        public virtual void BuildServices(IServiceBuilder serviceBuilder)
        {
        }

        /// <summary>
        /// Builds the pages in this package
        /// </summary>
        public virtual void BuildPages(IPageBuilder pageBuilder)
        {
        }

    }
}
