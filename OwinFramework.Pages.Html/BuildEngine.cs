using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Builders;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html
{
    public class BuildEngine: IBuildEngine
    {
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;
        private readonly IModuleDependenciesFactory _moduleDependenciesFactory;
        private readonly IPageDependenciesFactory _pageDependenciesFactory;
        private readonly ILayoutDependenciesFactory _layoutDependenciesFactory;
        private readonly IRegionDependenciesFactory _regionDependenciesFactory;
        private readonly IComponentDependenciesFactory _componentDependenciesFactory;
        private readonly IAssetManager _assetManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly IDataSupplierFactory _dataSupplierFactory;

        public BuildEngine(
            IRequestRouter requestRouter,
            INameManager nameManager,
            IModuleDependenciesFactory moduleDependenciesFactory,
            IPageDependenciesFactory pageDependenciesFactory,
            ILayoutDependenciesFactory layoutDependenciesFactory,
            IRegionDependenciesFactory regionDependenciesFactory,
            IComponentDependenciesFactory componentDependenciesFactory,
            IAssetManager assetManager,
            IHtmlHelper htmlHelper,
            IFluentBuilder fluentBuilder,
            IDataDependencyFactory dataDependencyFactory,
            IDataSupplierFactory dataSupplierFactory)
        {
            _requestRouter = requestRouter;
            _nameManager = nameManager;
            _moduleDependenciesFactory = moduleDependenciesFactory;
            _pageDependenciesFactory = pageDependenciesFactory;
            _layoutDependenciesFactory = layoutDependenciesFactory;
            _regionDependenciesFactory = regionDependenciesFactory;
            _componentDependenciesFactory = componentDependenciesFactory;
            _assetManager = assetManager;
            _htmlHelper = htmlHelper;
            _fluentBuilder = fluentBuilder;
            _dataDependencyFactory = dataDependencyFactory;
            _dataSupplierFactory = dataSupplierFactory;
        }

        public void Install(IFluentBuilder builder)
        {
            builder.ModuleBuilder = new ModuleBuilder(
                _moduleDependenciesFactory,
                _fluentBuilder);

            builder.PageBuilder = new PageBuilder(
                _requestRouter,
                _nameManager,
                _pageDependenciesFactory,
                _fluentBuilder);

            builder.LayoutBuilder = new LayoutBuilder(
                _nameManager,
                _htmlHelper,
                _layoutDependenciesFactory,
                _fluentBuilder);

            builder.RegionBuilder = new RegionBuilder(
                _nameManager,
                _htmlHelper,
                _regionDependenciesFactory,
                _fluentBuilder,
                _dataDependencyFactory,
                _dataSupplierFactory);

            builder.ComponentBuilder = new ComponentBuilder(
                _nameManager,
                _assetManager,
                _htmlHelper,
                _componentDependenciesFactory,
                _fluentBuilder);
        }
    }
}
