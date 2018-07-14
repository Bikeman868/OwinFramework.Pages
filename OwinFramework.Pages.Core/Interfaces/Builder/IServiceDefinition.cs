using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the fluent syntax for building services
    /// </summary>
    public interface IServiceDefinition
    {
        /// <summary>
        /// Sets the name of the module so that it can be referenced
        /// by page definitions
        /// </summary>
        IServiceDefinition Name(string name);

        /// <summary>
        /// Specifies that this service is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="package">The package that this service is
        /// part of</param>
        IServiceDefinition PartOf(IPackage package);

        /// <summary>
        /// Specifies that this service is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="packageName">The name of the package that this 
        /// service is part of</param>
        IServiceDefinition PartOf(string packageName);

        /// <summary>
        /// Specifies that this service is deployed as part of a module
        /// </summary>
        /// <param name="module">The module that this service is deployed in</param>
        IServiceDefinition DeployIn(IModule module);

        /// <summary>
        /// Specifies that this service is deployed as part of a module
        /// </summary>
        /// <param name="moduleName">The name of the module that this 
        /// service is deployed in</param>
        IServiceDefinition DeployIn(string moduleName);

        /// <summary>
        /// Overrides the default asset deployment scheme for this service
        /// </summary>
        IServiceDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Specifies the relative path to this page on the website
        /// </summary>
        /// <param name="path">The URL path to this page</param>
        /// <param name="priority">The priority is used to sort request filters.
        /// Higher priority filters execute before lower priority ones</param>
        /// <param name="methods">The http methods to route to this page</param>
        IPageDefinition Route(string path, int priority, params Methods[] methods);

        /// <summary>
        /// Specifies how to filter out requests to this page on the website
        /// </summary>
        /// <param name="filter">Serve this page for requests that match this filter</param>
        /// <param name="priority">Filters are evaluated from highest to lowest priority</param>
        IPageDefinition Route(IRequestFilter filter, int priority = 0);

        /// <summary>
        /// Adds metadata to the component that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <typeparam name="T">The type of data that this component binds to.
        /// Provides context for data binding expressions within the component</typeparam>
        IServiceDefinition BindTo<T>(string scope = null) where T : class;

        /// <summary>
        /// Adds metadata to the component that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        IServiceDefinition BindTo(Type dataType, string scope = null);

        /// <summary>
        /// Specifies the data scope. This will be used to identify the appropriate
        /// data providers
        /// </summary>
        IServiceDefinition DataScope(Type dataType, string scopeName);

        /// <summary>
        /// Specifies the name of a data provider. This is used as the first step in
        /// resolving data provision. If these providers do not provide all of the required
        /// data then there is a second step of finding data providers using the scope.
        /// </summary>
        /// <param name="providerName">The name of a specific data provider</param>
        IServiceDefinition DataProvider(string providerName);

        /// <summary>
        /// Specifies a data provider that is required to establish the data context for
        /// this page
        /// </summary>
        /// <param name="dataProvider">The data provider that is required for this page</param>
        IPageDefinition DataProvider(IDataProvider dataProvider);

        /// <summary>
        /// Builds the service
        /// </summary>
        IService Build();
    }
}
