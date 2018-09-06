namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Factory for data consumers
    /// </summary>
    public interface IDataContextBuilderFactory
    {
        /// <summary>
        /// Creates and initializes a new data context builder
        /// </summary>
        IDataContextBuilder Create(IDataScopeRules dataScopeRules);
    }
}
