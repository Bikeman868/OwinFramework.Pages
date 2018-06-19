using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    // TODO: Data binding
    // TODO: Render styles to dynamic assets
    // TODO: Implement AssetDeployment
    // TODO: Pages can override the contents of regions in the layout

    internal class LayoutBuilder : ILayoutBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly ILayoutDependenciesFactory _layoutDependenciesFactory;

        public LayoutBuilder(
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            ILayoutDependenciesFactory layoutDependenciesFactory)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _layoutDependenciesFactory = layoutDependenciesFactory;
        }

        ILayoutDefinition ILayoutBuilder.Layout()
        {
            return new LayoutDefinition(_nameManager, _htmlHelper, _layoutDependenciesFactory);
        }
    }
}
