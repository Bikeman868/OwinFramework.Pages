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
        /// Specifies that this component has a dependency on a specifc data context.
        /// You can call this multiple times to add more dependencies
        /// </summary>
        /// <param name="dataContextName">The name of the context handler that
        /// must be executed before rendering this component</param>
        IServiceDefinition DataContext(string dataContextName);

        /// <summary>
        /// Builds the service
        /// </summary>
        IService Build();
    }
}
