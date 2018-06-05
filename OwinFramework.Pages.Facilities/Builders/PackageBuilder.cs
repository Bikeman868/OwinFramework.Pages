using System;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Core.BaseClasses
{
    /// <summary>
    /// Inherit from this base class to define a package. You might want to 
    /// do this if you are building a reusable block of UI for other
    /// applications to consume, or if you want to create namespaces
    /// within a large application.
    /// </summary>
    public abstract class PackageBuilder
    {
        /// <summary>
        /// Assigns a package to a module defining the default deployment scheme 
        /// for everything in this package
        /// </summary>
        protected IModule Module;

        /// <summary>
        /// Use this to build the components in your package
        /// </summary>
        protected IComponentBuilder ComponentBuilder;

        /// <summary>
        /// Use this to build the regions in your package
        /// </summary>
        protected IRegionBuilder RegionBuilder;

        /// <summary>
        /// Use this to build the layouts in your package
        /// </summary>
        protected ILayoutBuilder LayoutBuilder;

        /// <summary>
        /// Use this to build the services in your package
        /// </summary>
        protected IServiceBuilder ServiceBuilder;

        /// <summary>
        /// Use this to build the pages in your package
        /// </summary>
        protected IPageBuilder PageBuilder;

        public abstract void Build();

    }
}
