namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// This interface is implemented by elements that can establish a new
    /// data scope during the rendering operation.
    /// </summary>
    public interface IDataScopeProviderFactory
    {
        /// <summary>
        /// Creates and initializes a new data scope provider
        /// </summary>
        IDataScopeProvider Create(IDataScopeProvider parent);
    }
}
