using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataScopeProviderFactory: IDataScopeProviderFactory
    {
        private readonly IDataScopeFactory _dataScopeFactory;

        public DataScopeProviderFactory(IDataScopeFactory dataScopeFactory)
        {
            _dataScopeFactory = dataScopeFactory;
        }

        public IDataScopeRules Create()
        {
            return new DataScopeRules(_dataScopeFactory);
        }
    }
}
