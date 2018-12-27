using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// The IoC dependencies are wrapped in this factory so that when
    /// new dependencies are added it does not change the constructor
    /// which would break any application code that inherits from it
    /// </summary>
    public interface IRegionDependenciesFactory
    {
        /// <summary>
        /// Constructs region dependencies specific to a request
        /// </summary>
        IRegionDependencies Create(IOwinContext context);

        /// <summary>
        /// A factory for constructing data scope providers
        /// </summary>
        IDataScopeProviderFactory DataScopeProviderFactory { get; }

        /// <summary>
        /// Returns a factory that can construct data consumer mixins
        /// </summary>
        IDataConsumerFactory DataConsumerFactory { get; }

        /// <summary>
        /// Returns a factory that can construct data dependencies
        /// </summary>
        IDataDependencyFactory DataDependencyFactory { get; }

        /// <summary>
        /// Returns a factory that can construct data suppliers
        /// </summary>
        IDataSupplierFactory DataSupplierFactory { get; }

        /// <summary>
        /// Returns a factory that can construct data scopes
        /// </summary>
        IDataScopeFactory DataScopeFactory { get; }

        /// <summary>
        /// Returns a factory that can construct css writers
        /// </summary>
        ICssWriterFactory CssWriterFactory { get; }

        /// <summary>
        /// Returns a factory that can construct css writers
        /// </summary>
        IJavascriptWriterFactory JavascriptWriterFactory { get; }
    }
}
