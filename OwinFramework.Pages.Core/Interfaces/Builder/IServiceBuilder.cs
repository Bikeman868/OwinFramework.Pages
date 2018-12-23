using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the service builder to construct a class that will respond to service requests
    /// </summary>
    public interface IServiceBuilder
    {
        /// <summary>
        /// Starts building a new service or configuring an existing service
        /// </summary>
        /// <param name="serviceInstance">Pass an instance of the Service class to 
        /// configure an instance of a derrived class or pass null to construct an
        /// instance of the Service class</param>
        /// <param name="declaringType">Type type to extract custom attributes from
        /// that can also define the behaviour of the service</param>
        /// <param name="factory">If you do not provide a service instance and the
        /// declaring type does not have a default public constructor, then you must
        /// provide a factory method that can construct the service.</param>
        /// <param name="package">Optional package adds a namespace to this service</param>
        IServiceDefinition BuildUpService(
            object serviceInstance = null, 
            Type declaringType = null, 
            Func<Type, object> factory = null,
            IPackage package = null);
    }
}
