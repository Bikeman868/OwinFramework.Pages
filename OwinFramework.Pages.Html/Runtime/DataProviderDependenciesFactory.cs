using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class DataProviderDependenciesFactory : IDataProviderDependenciesFactory
    {
        public IDataConsumerFactory DataConsumerFactory { get; private set; }

        public DataProviderDependenciesFactory(
            IDataConsumerFactory dataConsumerFactory)
        {
            DataConsumerFactory = dataConsumerFactory;
        }

        public IDataProviderDependencies Create(IOwinContext context)
        {
            return new DataProviderDependencies();
        }

    }
}
