namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the component builder to construct components using a fluent syntax
    /// </summary>
    public interface IComponentBuilder
    {
        /// <summary>
        /// Starts building a new component
        /// </summary>
        IComponentDefinition Component(IPackage package = null);
    }
}
