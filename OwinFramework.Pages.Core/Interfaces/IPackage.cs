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
        /// Packages build their components here
        /// </summary>
        void BuildComponents(IComponentBuilder componentBuilder);

        /// <summary>
        /// Packages build their regions here
        /// </summary>
        void BuildRegions(IRegionBuilder regionBuilder);

        /// <summary>
        /// Packages build their layouts here
        /// </summary>
        void BuildLayouts(ILayoutBuilder layoutBuilder);

        /// <summary>
        /// Packages build their services here
        /// </summary>
        void BuildServices(IServiceBuilder serviceBuilder);

        /// <summary>
        /// Packages build their pages here
        /// </summary>
        void BuildPages(IPageBuilder pageBuilder);
    }
}
