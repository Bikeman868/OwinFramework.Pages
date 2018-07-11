using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class DataProviderDependenciesFactory: IDataProviderDependenciesFactory
    {
        public IDataConsumerFactory DataConsumerFactory { get; private set; }
        public IDataSupplierFactory DataSupplierFactory { get; private set; }
        public IDataDependencyFactory DataDependencyFactory { get; private set; }

        public DataProviderDependenciesFactory(
            IDataConsumerFactory dataConsumerFactory,
            IDataSupplierFactory dataSupplierFactory,
            IDataDependencyFactory dataDependencyFactory)
        {
            DataConsumerFactory = dataConsumerFactory;
            DataSupplierFactory = dataSupplierFactory;
            DataDependencyFactory = dataDependencyFactory;
        }

        IDataProviderDependencies IDataProviderDependenciesFactory.Create(IOwinContext context)
        {
            return new DataProviderDependencies().Initialize(context);
        }
    }
}
