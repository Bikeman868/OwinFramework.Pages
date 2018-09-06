using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataContextBuilderFactory : IDataContextBuilderFactory
    {
        private readonly IDataContextFactory _dataContextFactory;
        private readonly IIdManager _idManager;
        private readonly IDataCatalog _dataCatalog;

        public DataContextBuilderFactory(
            IDataContextFactory dataContextFactory,
            IIdManager idManager,
            IDataCatalog dataCatalog)
        {
            _dataContextFactory = dataContextFactory;
            _idManager = idManager;
            _dataCatalog = dataCatalog;
        }

        public IDataContextBuilder Create(IDataScopeRules dataScopeRules)
        {
            return new DataContextBuilder(
                _dataContextFactory,
                _idManager,
                _dataCatalog,
                dataScopeRules);
        }
    }
}
