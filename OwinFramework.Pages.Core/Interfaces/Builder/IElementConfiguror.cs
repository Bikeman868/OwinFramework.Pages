using System;
using OwinFramework.Pages.Core.Attributes;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Knows how to take custom attributes and apply them to 
    /// classes that implement certain interfaces
    /// </summary>
    public interface IElementConfiguror
    {
        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a component using the component defining fluent syntax
        /// </summary>
        void Configure(IComponentDefinition component, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a data provider using the data provider defining fluent syntax
        /// </summary>
        void Configure(IDataProviderDefinition dataProvider, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a layout using the layout defining fluent syntax
        /// </summary>
        void Configure(ILayoutDefinition layout, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a module using the module defining fluent syntax
        /// </summary>
        void Configure(IModuleDefinition module, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a package using the package defining fluent syntax
        /// </summary>
        void Configure(IPackageDefinition package, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a page using the page defining fluent syntax
        /// </summary>
        void Configure(IPageDefinition page, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a region using the region defining fluent syntax
        /// </summary>
        void Configure(IRegionDefinition region, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a service using the service defining fluent syntax
        /// </summary>
        void Configure(IServiceDefinition service, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a component using the component defining fluent syntax
        /// </summary>
        void Configure(IComponent component, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a data provider using the data provider defining fluent syntax
        /// </summary>
        void Configure(IDataProvider dataProvider, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a layout using the layout defining fluent syntax
        /// </summary>
        void Configure(ILayout layout, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a module using the module defining fluent syntax
        /// </summary>
        void Configure(IModule module, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a package using the package defining fluent syntax
        /// </summary>
        void Configure(IPackage package, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a page using the page defining fluent syntax
        /// </summary>
        void Configure(IPage page, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a region using the region defining fluent syntax
        /// </summary>
        void Configure(IRegion region, AttributeSet attributes);

        /// <summary>
        /// Uses custom attributes from the declaring type
        /// to configure a service using the service defining fluent syntax
        /// </summary>
        void Configure(IService service, AttributeSet attributes);
    }
}
