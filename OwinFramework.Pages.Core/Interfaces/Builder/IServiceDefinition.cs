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
        /// Overrides the default asset deployment scheme for this service
        /// </summary>
        IServiceDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Builds the service
        /// </summary>
        IService Build();
    }
}
