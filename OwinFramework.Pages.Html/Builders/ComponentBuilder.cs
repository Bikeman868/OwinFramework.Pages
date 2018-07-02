using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ComponentBuilder : IComponentBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IAssetManager _assetManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IComponentDependenciesFactory _componentDependenciesFactory;
        private readonly IFluentBuilder _fluentBuilder;

        public ComponentBuilder(
            INameManager nameManager,
            IAssetManager assetManager,
            IHtmlHelper htmlHelper,
            IComponentDependenciesFactory componentDependenciesFactory,
            IFluentBuilder fluentBuilder)

        {
            _nameManager = nameManager;
            _assetManager = assetManager;
            _htmlHelper = htmlHelper;
            _componentDependenciesFactory = componentDependenciesFactory;
            _fluentBuilder = fluentBuilder;
        }

        IComponentDefinition IComponentBuilder.Component(IPackage package)
        {
            return new ComponentDefinition(_nameManager, _assetManager, _htmlHelper, _fluentBuilder, _componentDependenciesFactory, package);
        }
    }
}
