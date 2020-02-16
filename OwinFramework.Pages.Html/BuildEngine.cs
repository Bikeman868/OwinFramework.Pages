using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Builders;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html
{
    /// <summary>
    /// This build engine provides builders for modules, pages, layouts,
    /// regions and components
    /// </summary>
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
        private readonly IElementConfiguror _elementConfiguror;
        private readonly IFrameworkConfiguration _frameworkConfiguration;

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
            IElementConfiguror elementConfiguror,
            IFrameworkConfiguration frameworkConfiguration)
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
            _elementConfiguror = elementConfiguror;
            _frameworkConfiguration = frameworkConfiguration;
        }

        public void Install(IFluentBuilder builder)
        {
            builder.ModuleBuilder = new ModuleBuilder(
                _moduleDependenciesFactory,
                _elementConfiguror,
                builder);

            builder.PageBuilder = new PageBuilder(
                _requestRouter,
                _nameManager,
                _pageDependenciesFactory,
                _componentDependenciesFactory,
                _elementConfiguror,
                builder);

            builder.LayoutBuilder = new LayoutBuilder(
                _layoutDependenciesFactory,
                _regionDependenciesFactory,
                _componentDependenciesFactory,
                _nameManager,
                _htmlHelper,
                _elementConfiguror,
                builder);

            builder.RegionBuilder = new RegionBuilder(
                _nameManager,
                _htmlHelper,
                _regionDependenciesFactory,
                _componentDependenciesFactory,
                _elementConfiguror,
                builder);

            builder.ComponentBuilder = new ComponentBuilder(
                _nameManager,
                _assetManager,
                _htmlHelper,
                _componentDependenciesFactory,
                _elementConfiguror,
                _frameworkConfiguration,
                builder);
        }
    }
}
