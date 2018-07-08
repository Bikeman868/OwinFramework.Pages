using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class RegionDependenciesFactory: IRegionDependenciesFactory
    {
        public IDataScopeProviderFactory DataScopeProviderFactory { get; private set; }
        public IDataConsumerFactory DataConsumerFactory { get; private set; }
        public IDataDependencyFactory DataDependencyFactory { get; private set; }
        public IDataSupplierFactory DataSupplierFactory { get; private set; }

        public RegionDependenciesFactory(
            IDataScopeProviderFactory dataScopeProviderFactory,
            IDataConsumerFactory dataConsumerFactory,
            IDataDependencyFactory dataDependencyFactory,
            IDataSupplierFactory dataSupplierFactory)
        {
            DataScopeProviderFactory = dataScopeProviderFactory;
            DataConsumerFactory = dataConsumerFactory;
            DataDependencyFactory = dataDependencyFactory;
            DataSupplierFactory = dataSupplierFactory;
        }

        public IRegionDependencies Create(IOwinContext context)
        {
            return new RegionDependencies();
        }
    }
}
