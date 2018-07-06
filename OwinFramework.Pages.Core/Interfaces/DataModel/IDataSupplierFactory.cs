namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Factory for IDataProviderDefinition
    /// </summary>
    public interface IDataSupplierFactory
    {
        /// <summary>
        /// Constructs and initializes a new data supplier
        /// </summary>
        IDataSupplier Create();
    }
}
