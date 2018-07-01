using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Container for the parameters required to call a data provider
    /// </summary>
    public interface IDataProviderDefinition
    {
        /// <summary>
        /// The data provider to call
        /// </summary>
        IDataProvider DataProvider { get; set; }

        /// <summary>
        /// Optional dependency that the provider should satisfy
        /// </summary>
        IDataDependency Dependency { get; set; }

        /// <summary>
        /// Adds data into the data context by executing the data provider
        /// </summary>
        /// <param name="renderContext">The render context to pass to
        /// the data provider. The provider will use this to get information
        /// about the request that was made</param>
        /// <param name="dataContext">The data context to add data to, note that
        /// this might not be the current data context in the render context</param>
        void Execute(IRenderContext renderContext, IDataContext dataContext);
    }
}
