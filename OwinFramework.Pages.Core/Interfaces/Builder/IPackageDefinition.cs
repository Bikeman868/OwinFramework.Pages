namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the fluent syntax for building modules
    /// </summary>
    public interface IPackageDefinition
    {
        /// <summary>
        /// Sets the name of the package so that it can be referenced
        /// by other elements
        /// </summary>
        IPackageDefinition Name(string name);

        /// <summary>
        /// Overrides the default asset deployment scheme for this module
        /// </summary>
        IPackageDefinition NamespaceName(string namespaceName);

        /// <summary>
        /// Sets the default module for all elements in this package
        /// </summary>
        IPackageDefinition Module(IModule module);

        /// <summary>
        /// Sets the default module for all elements in this package
        /// </summary>
        IPackageDefinition Module(string moduleName);

        /// <summary>
        /// Builds the module
        /// </summary>
        IPackage Build();
    }
}
