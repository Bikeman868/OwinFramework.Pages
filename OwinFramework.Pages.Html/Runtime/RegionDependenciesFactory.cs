using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class RegionDependenciesFactory: IRegionDependenciesFactory
    {
        public IDataScopeProviderFactory DataScopeProviderFactory { get; private set; }
        public IDataConsumerFactory DataConsumerFactory { get; private set; }
        public IDataDependencyFactory DataDependencyFactory { get; private set; }
        public IDataSupplierFactory DataSupplierFactory { get; private set; }
        public IDataScopeFactory DataScopeFactory { get; private set; }
        public ICssWriterFactory CssWriterFactory { get; private set; }
        public IJavascriptWriterFactory JavascriptWriterFactory { get; private set; }

        public RegionDependenciesFactory(
            IDataScopeProviderFactory dataScopeProviderFactory,
            IDataConsumerFactory dataConsumerFactory,
            IDataDependencyFactory dataDependencyFactory,
            IDataSupplierFactory dataSupplierFactory,
            IDataScopeFactory dataScopeFactory,
            ICssWriterFactory cssWriterFactory,
            IJavascriptWriterFactory javascriptWriterFactory)
        {
            DataScopeProviderFactory = dataScopeProviderFactory;
            DataConsumerFactory = dataConsumerFactory;
            DataDependencyFactory = dataDependencyFactory;
            DataSupplierFactory = dataSupplierFactory;
            DataScopeFactory = dataScopeFactory;
            CssWriterFactory = cssWriterFactory;
            JavascriptWriterFactory = javascriptWriterFactory;
        }

        public IRegionDependencies Create(IOwinContext context)
        {
            return new RegionDependencies();
        }
    }
}
