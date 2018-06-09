using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Facilities.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class PageDependenciesFactory: IPageDependenciesFactory
    {
        private readonly IRenderContextFactory _renderContextFactory;
        private readonly IDataContextFactory _dataContextFactory;
        private readonly IAssetManager _assetManager;
        private readonly INameManager _nameManager;

        public PageDependenciesFactory(
            IRenderContextFactory renderContextFactory,
            IDataContextFactory dataContextFactory,
            IAssetManager assetManager,
            INameManager nameManager)
        {
            _renderContextFactory = renderContextFactory;
            _dataContextFactory = dataContextFactory;
            _assetManager = assetManager;
            _nameManager = nameManager;
        }

        public IPageDependencies Create(IOwinContext context)
        {
            return new PageDependencies(
                _renderContextFactory.Create(),
                _dataContextFactory.Create(),
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
    }
}
