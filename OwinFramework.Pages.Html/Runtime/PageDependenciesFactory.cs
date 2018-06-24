using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class PageDependenciesFactory: IPageDependenciesFactory
    {
        private readonly IRenderContextFactory _renderContextFactory;
        private readonly IDataContextFactory _dataContextFactory;
        private readonly IAssetManager _assetManager;
        private readonly INameManager _nameManager;
        private readonly ICssWriterFactory _cssWriterFactory;
        private readonly IJavascriptWriterFactory _javascriptWriterFactory;

        public PageDependenciesFactory(
            IRenderContextFactory renderContextFactory,
            IDataContextFactory dataContextFactory,
            IAssetManager assetManager,
            INameManager nameManager,
            ICssWriterFactory cssWriterFactory,
            IJavascriptWriterFactory javascriptWriterFactory)
        {
            _renderContextFactory = renderContextFactory;
            _dataContextFactory = dataContextFactory;
            _assetManager = assetManager;
            _nameManager = nameManager;
            _cssWriterFactory = cssWriterFactory;
            _javascriptWriterFactory = javascriptWriterFactory;
        }

        public IPageDependencies Create(IOwinContext context)
        {
            var renderContext = _renderContextFactory.Create();
            return new PageDependencies(
                renderContext,
                _dataContextFactory.Create(renderContext),
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
    }
}
