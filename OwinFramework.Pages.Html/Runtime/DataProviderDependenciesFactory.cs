using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class DataProviderDependenciesFactory : IDataProviderDependenciesFactory
    {
        public IDataProviderDependencies Create()
        {
            return new DataProviderDependencies();
        }
    }
}
