﻿using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class PageDependenciesFactory: IPageDependenciesFactory
    {
        private readonly IRenderContextFactory _renderContextFactory;
        private readonly IAssetManager _assetManager;
        private readonly INameManager _nameManager;
        private readonly ICssWriterFactory _cssWriterFactory;
        private readonly IJavascriptWriterFactory _javascriptWriterFactory;
        private readonly IDataScopeProviderFactory _dataScopeProviderFactory;
        private readonly IDataConsumerFactory _dataConsumerFactory;

        public PageDependenciesFactory(
            IRenderContextFactory renderContextFactory,
            IAssetManager assetManager,
            INameManager nameManager,
            ICssWriterFactory cssWriterFactory,
            IJavascriptWriterFactory javascriptWriterFactory,
            IDataScopeProviderFactory dataScopeProviderFactory, 
            IDataConsumerFactory dataConsumerFactory)
        {
            _renderContextFactory = renderContextFactory;
            _assetManager = assetManager;
            _nameManager = nameManager;
            _cssWriterFactory = cssWriterFactory;
            _javascriptWriterFactory = javascriptWriterFactory;
            _dataScopeProviderFactory = dataScopeProviderFactory;
            _dataConsumerFactory = dataConsumerFactory;
        }

        public IPageDependencies Create(IOwinContext context)
        {
            var renderContext = _renderContextFactory.Create();
            return new PageDependencies(
                renderContext,
                _assetManager,
                _nameManager)
                .Initialize(context);
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
    }
}
