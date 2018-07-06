using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// The IoC dependencies are wrapped in this factory so that when
    /// new dependencies are added it does not change the constructor
    /// which would break any application code that inherits from it
    /// </summary>
    public interface IDataProviderDependenciesFactory
    {
        /// <summary>
        /// A factory for constructing data consumers
        /// </summary>
        IDataConsumerFactory DataConsumerFactory { get; }

        /// <summary>
        /// A factory for constructing data suppliers
        /// </summary>
        IDataSupplierFactory DataSupplierFactory { get; }

        /// <summary>
        /// Constructs and initializes a data provider dependencies 
        /// instance specifci to a request
        /// </summary>
        IDataProviderDependencies Create(IOwinContext context);
    }
}
