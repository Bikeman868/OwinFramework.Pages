using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;

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
        private readonly IFluentBuilder _fluentBuilder;

        public LayoutBuilder(
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            ILayoutDependenciesFactory layoutDependenciesFactory,
            IFluentBuilder fluentBuilder)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _layoutDependenciesFactory = layoutDependenciesFactory;
            _fluentBuilder = fluentBuilder;
        }

        ILayoutDefinition ILayoutBuilder.Layout(Type declaringType, IPackage package)
        {
            return new LayoutDefinition(declaringType, _nameManager, _htmlHelper, _fluentBuilder, _layoutDependenciesFactory, package);
        }
    }
}
