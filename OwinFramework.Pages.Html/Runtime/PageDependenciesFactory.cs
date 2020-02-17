using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class PageDependenciesFactory: IPageDependenciesFactory
    {
        private readonly IRenderContextFactory _renderContextFactory;
        private readonly IIdManager _idManager;
        private readonly IAssetManager _assetManager;
        private readonly INameManager _nameManager;
        private readonly ICssWriterFactory _cssWriterFactory;
        private readonly IJavascriptWriterFactory _javascriptWriterFactory;
        private readonly IDataScopeProviderFactory _dataScopeProviderFactory;
        private readonly IDataConsumerFactory _dataConsumerFactory;
        private readonly IDictionaryFactory _dictionaryFactory;
        private readonly IDataContextBuilderFactory _dataContextBuilderFactory;
        private readonly IDataCatalog _dataCatalog;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly IFrameworkConfiguration _frameworkConfiguration;

        public PageDependenciesFactory(
            IRenderContextFactory renderContextFactory,
            IIdManager idManager,
            IAssetManager assetManager,
            INameManager nameManager,
            ICssWriterFactory cssWriterFactory,
            IJavascriptWriterFactory javascriptWriterFactory,
            IDataScopeProviderFactory dataScopeProviderFactory, 
            IDataConsumerFactory dataConsumerFactory,
            IDictionaryFactory dictionaryFactory,
            IDataContextBuilderFactory dataContextBuilderFactory,
            IDataCatalog dataCatalog, 
            IDataDependencyFactory dataDependencyFactory,
            IFrameworkConfiguration frameworkConfiguration)
        {
            _renderContextFactory = renderContextFactory;
            _idManager = idManager;
            _assetManager = assetManager;
            _nameManager = nameManager;
            _cssWriterFactory = cssWriterFactory;
            _javascriptWriterFactory = javascriptWriterFactory;
            _dataScopeProviderFactory = dataScopeProviderFactory;
            _dataConsumerFactory = dataConsumerFactory;
            _dictionaryFactory = dictionaryFactory;
            _dataContextBuilderFactory = dataContextBuilderFactory;
            _dataCatalog = dataCatalog;
            _dataDependencyFactory = dataDependencyFactory;
            _frameworkConfiguration = frameworkConfiguration;
        }

        public IPageDependencies Create(IOwinContext context, Action<IOwinContext, Func<string>> trace)
        {
            var renderContext = _renderContextFactory.Create(trace);

            return new PageDependencies(
                renderContext,
                _assetManager,
                _nameManager)
                .Initialize(context);
        }

        public IIdManager IdManager => _idManager;
        public INameManager NameManager => _nameManager;
        public IAssetManager AssetManager => _assetManager;
        public ICssWriterFactory CssWriterFactory => _cssWriterFactory;
        public IJavascriptWriterFactory JavascriptWriterFactory => _javascriptWriterFactory;
        public IDataScopeProviderFactory DataScopeProviderFactory => _dataScopeProviderFactory;
        public IDataConsumerFactory DataConsumerFactory => _dataConsumerFactory;
        public IDictionaryFactory DictionaryFactory => _dictionaryFactory;
        public IDataContextBuilderFactory DataContextBuilderFactory => _dataContextBuilderFactory;
        public IDataCatalog DataCatalog => _dataCatalog;
        public IDataDependencyFactory DataDependencyFactory => _dataDependencyFactory;
        public IFrameworkConfiguration FrameworkConfiguration => _frameworkConfiguration;
    }
}
