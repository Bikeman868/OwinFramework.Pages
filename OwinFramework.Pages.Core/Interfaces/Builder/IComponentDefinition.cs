using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the fluent syntax for building components
    /// </summary>
    public interface IComponentDefinition
    {
        /// <summary>
        /// Sets the name of the component so that it can be referenced
        /// by other elements
        /// </summary>
        IComponentDefinition Name(string name);

        /// <summary>
        /// Overrides the default asset deployment scheme for this component
        /// </summary>
        IComponentDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Builds the module
        /// </summary>
        IComponent Build();
    }
}
