namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// A factory for creating data contexts. The data context is used
    /// to connect sources of data to the frameworks that render output.
    /// </summary>
    public interface IDataContextFactory
    {
        /// <summary>
        /// Creates and initializes a new data context instance
        /// </summary>
        IDataContext Create(IRenderContext renderContext);
    }
}
