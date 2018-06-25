using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class RegionDependenciesFactory: IRegionDependenciesFactory
    {
        public IDataScopeProviderFactory DataScopeProviderFactory { get; private set; }

        public RegionDependenciesFactory(
            IDataScopeProviderFactory dataScopeProviderFactory)
        {
            DataScopeProviderFactory = dataScopeProviderFactory;
        }

        public IRegionDependencies Create()
        {
            return new RegionDependencies();
        }

    }
}
