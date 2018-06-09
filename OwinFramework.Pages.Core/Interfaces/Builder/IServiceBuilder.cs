using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the service builder to construct a class that will respond to service requests
    /// </summary>
    public interface IServiceBuilder
    {
        /// <summary>
        /// Starts building a new service
        /// </summary>
        IServiceDefinition Service(Type declaringType = null);
    }
}
