﻿using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataScopeProviderFactory: IDataScopeProviderFactory
    {
        private readonly IIdManager _idManager;
        private readonly IDataScopeFactory _dataScopeFactory;
        private readonly IDataCatalog _dataCatalog;
        private readonly IDataContextFactory _dataContextFactory;
        private readonly IDataProviderDefinitionFactory _dataProviderDefinitionFactory;

        public DataScopeProviderFactory(
            IIdManager idManager,
            IDataScopeFactory dataScopeFactory,
            IDataCatalog dataCatalog,
            IDataContextFactory dataContextFactory,
            IDataProviderDefinitionFactory dataProviderDefinitionFactory)
        {
            _idManager = idManager;
            _dataScopeFactory = dataScopeFactory;
            _dataCatalog = dataCatalog;
            _dataContextFactory = dataContextFactory;
            _dataProviderDefinitionFactory = dataProviderDefinitionFactory;
            _dataScopeFactory = dataScopeFactory;
            _dataCatalog = dataCatalog;
            _dataContextFactory = dataContextFactory;
        }

        public IDataScopeProvider Create(IDataScopeProvider parent)
        {
            return new DataScopeProvider(
                _idManager,
                _dataScopeFactory,
                _dataProviderDefinitionFactory,
                _dataCatalog,
                _dataContextFactory,
                parent);
        }
    }
}