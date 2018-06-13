using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Builders;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html
{
    public class BuildEngine: IBuildEngine
    {
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;
        private readonly IPageDependenciesFactory _pageDependenciesFactory;
        private readonly IAssetManager _assetManager;
        private readonly IHtmlHelper _htmlHelper;

        public BuildEngine(
            IRequestRouter requestRouter,
            INameManager nameManager,
            IPageDependenciesFactory pageDependenciesFactory,
            IAssetManager assetManager,
            IHtmlHelper htmlHelper)
        {
            _requestRouter = requestRouter;
            _nameManager = nameManager;
            _pageDependenciesFactory = pageDependenciesFactory;
            _assetManager = assetManager;
            _htmlHelper = htmlHelper;
        }

        public void Install(IFluentBuilder builder)
        {
            builder.ModuleBuilder = new ModuleBuilder(
                _nameManager);

            builder.PageBuilder = new PageBuilder(
                _requestRouter,
                _nameManager,
                _pageDependenciesFactory);

            builder.LayoutBuilder = new LayoutBuilder(
                _nameManager,
                _htmlHelper);

            builder.RegionBuilder = new RegionBuilder(
                _nameManager,
                _htmlHelper);

            builder.ComponentBuilder = new ComponentBuilder(
                _nameManager,
                _assetManager);
        }
    }
}
