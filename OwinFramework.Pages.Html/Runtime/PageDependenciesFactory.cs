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
        private readonly IDataContextFactory _dataContextFactory;

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
            IDataContextFactory dataContextFactory)
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
            _dataContextFactory = dataContextFactory;
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

        public IIdManager IdManager
        {
            get { return _idManager; }
        }

        public INameManager NameManager
        {
            get { return _nameManager; }
        }

        public IAssetManager AssetManager
        {
            get { return _assetManager; }
        }

        public ICssWriterFactory CssWriterFactory
        {
            get { return _cssWriterFactory; }
        }

        public IJavascriptWriterFactory JavascriptWriterFactory
        {
            get { return _javascriptWriterFactory; }
        }

        public IDataScopeProviderFactory DataScopeProviderFactory
        {
            get { return _dataScopeProviderFactory; }
        }

        public IDataConsumerFactory DataConsumerFactory
        {
            get { return _dataConsumerFactory; }
        }

        public IDictionaryFactory DictionaryFactory
        {
            get { return _dictionaryFactory; }
        }

        public IDataContextFactory DataContextFactory
        {
            get { return _dataContextFactory; }
        }
    }
}
