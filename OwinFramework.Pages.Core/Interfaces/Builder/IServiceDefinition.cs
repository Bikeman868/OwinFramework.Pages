using System;
using OwinFramework.InterfacesV1.Middleware;
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
        /// Defines the permission needed to call this service
        /// </summary>
        /// <param name="requiredPermission">The name of a required permission or null</param>
        /// <param name="endpointSpecificPermission">True if the service/endpoint should be 
        /// used as an asset path when checking permissions</param>
        /// <returns></returns>
        IServiceDefinition RequiredPermission(string requiredPermission, bool endpointSpecificPermission);

        /// <summary>
        /// Defines that the user must identify with the system to make this service call
        /// </summary>
        IServiceDefinition RequireIdentification();

        /// <summary>
        /// Defines the default serialization for endpoints in this service
        /// </summary>
        /// <param name="requestDeserializer">The type that deserializes the request body</param>
        /// <param name="responseSerializer">The type that serializes the response body</param>
        /// <returns></returns>
        IServiceDefinition Serialization(Type requestDeserializer, Type responseSerializer);

        /// <summary>
        /// Instructs the output cache how to cache output from this service
        /// </summary>
        /// <param name="category">Cache category name</param>
        /// <param name="priority">Cache priority name</param>
        IServiceDefinition Cache(string category, CachePriority priority);

        /// <summary>
        /// Sets the route to the endpoints in this service
        /// </summary>
        /// <param name="basePath">The base path of endpoints in this service</param>
        /// <param name="methods">The http methods to route to this service</param>
        /// <param name="priority">Routes are matched to the request in order of priority</param>
        IServiceDefinition Route(string basePath, Method[] methods, int priority);

        /// <summary>
        /// Builds the service
        /// </summary>
        IService Build();
    }
}
