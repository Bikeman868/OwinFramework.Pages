using System;
using OwinFramework.Pages.Core.Enums;

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
        /// Adds metadata to the component that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <typeparam name="T">The type of data that this component binds to.
        /// Provides context for data binding expressions within the component</typeparam>
        IServiceDefinition BindTo<T>() where T : class;

        /// <summary>
        /// Adds metadata to the component that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        IServiceDefinition BindTo(Type dataType);

        /// <summary>
        /// Specifies the data scope. This will be used to identify the appropriate
        /// data providers
        /// </summary>
        /// <param name="scopeName">The name of the data scope to use when resolving data providers</param>
        IServiceDefinition DataScope(string scopeName);

        /// <summary>
        /// Specifies the name of a data provider. This is used as the first step in
        /// resolving data provision. If these providers do not provide all of the required
        /// data then there is a second step of finding data providers using the scope.
        /// </summary>
        /// <param name="providerName">The name of a specific data provider</param>
        IServiceDefinition DataProvider(string providerName);

        /// <summary>
        /// Builds the service
        /// </summary>
        IService Build();
    }
}
