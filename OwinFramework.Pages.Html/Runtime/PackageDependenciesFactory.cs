using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class PackageDependenciesFactory: IPackageDependenciesFactory
    {
        public INameManager NameManager { get { return _nameManager; } }
        public IAssetManager AssetManager { get { return _assetManager; } }

        public IModuleDependenciesFactory ModuleDependenciesFactory { get { return _moduleDependenciesFactory; } }
        public IPageDependenciesFactory PageDependenciesFactory { get { return _pageDependenciesFactory; } }
        public ILayoutDependenciesFactory LayoutDependenciesFactory { get { return _layoutDependenciesFactory; } }
        public IRegionDependenciesFactory RegionDependenciesFactory { get { return _regionDependenciesFactory; } }
        public IComponentDependenciesFactory ComponentDependenciesFactory { get { return _componentDependenciesFactory; } }

        private readonly IRenderContextFactory _renderContextFactory;
        private readonly IDataContextFactory _dataContextFactory;
        private readonly IAssetManager _assetManager;
        private readonly INameManager _nameManager;

        private readonly IModuleDependenciesFactory _moduleDependenciesFactory;
        private readonly IPageDependenciesFactory _pageDependenciesFactory;
        private readonly ILayoutDependenciesFactory _layoutDependenciesFactory;
        private readonly IRegionDependenciesFactory _regionDependenciesFactory;
        private readonly IComponentDependenciesFactory _componentDependenciesFactory;

        public PackageDependenciesFactory(
            IRenderContextFactory renderContextFactory,
            IDataContextFactory dataContextFactory,
            IAssetManager assetManager,
            INameManager nameManager,
            IModuleDependenciesFactory moduleDependenciesFactory,
            IPageDependenciesFactory pageDependenciesFactory,
            ILayoutDependenciesFactory layoutDependenciesFactory,
            IRegionDependenciesFactory regionDependenciesFactory,
            IComponentDependenciesFactory componentDependenciesFactory)
        {
            _renderContextFactory = renderContextFactory;
            _dataContextFactory = dataContextFactory;
            _assetManager = assetManager;
            _nameManager = nameManager;
            _moduleDependenciesFactory = moduleDependenciesFactory;
            _pageDependenciesFactory = pageDependenciesFactory;
            _layoutDependenciesFactory = layoutDependenciesFactory;
            _regionDependenciesFactory = regionDependenciesFactory;
            _componentDependenciesFactory = componentDependenciesFactory;
        }

        public IPackageDependencies Create(IOwinContext context)
        {
            return new PackageDependencies(
                _renderContextFactory.Create(),
                _dataContextFactory.Create(),
                _assetManager,
                _nameManager,
                _moduleDependenciesFactory,
                _pageDependenciesFactory,
                _layoutDependenciesFactory,
                _regionDependenciesFactory,
                _componentDependenciesFactory)
                .Initialize(context);
        }
    }
}
