using System;
using System.Linq;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ComponentBuilder : IComponentBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IAssetManager _assetManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IComponentDependenciesFactory _componentDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;
        private readonly IFluentBuilder _fluentBuilder;

        public ComponentBuilder(
            INameManager nameManager,
            IAssetManager assetManager,
            IHtmlHelper htmlHelper,
            IComponentDependenciesFactory componentDependenciesFactory,
            IElementConfiguror elementConfiguror,
            IFluentBuilder fluentBuilder)

        {
            _nameManager = nameManager;
            _assetManager = assetManager;
            _htmlHelper = htmlHelper;
            _componentDependenciesFactory = componentDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _fluentBuilder = fluentBuilder;
        }

        IComponentDefinition IComponentBuilder.BuildUpComponent(object componentInstance, Type declaringType, IPackage package)
        {
            var component = componentInstance as Component ?? new Component(_componentDependenciesFactory);
            if (declaringType == null) declaringType = (componentInstance ?? component).GetType();

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(component, attributes);

            var componentDefinition = new ComponentDefinition(component, _nameManager, _assetManager, _htmlHelper, _fluentBuilder, package);
            Configure(componentDefinition, attributes);

            return componentDefinition;
        }

        private void Configure(IComponentDefinition componentDefinition, AttributeSet attributes)
        {
            // TODO: Check that these are not already handled by IElementConfiguror

            if (attributes.RenderHtmls != null)
            {
                foreach (var renderHtml in attributes.RenderHtmls.OrderBy(r => r.Order))
                    componentDefinition.Render(renderHtml.TextName, renderHtml.Html);
            }

            if (attributes.NeedsComponents != null)
            {
                foreach (var neededComponent in attributes.NeedsComponents)
                    componentDefinition.NeedsComponent(neededComponent.ComponentName);
            }

            if (attributes.DeployCsss != null)
                foreach (var deployCss in attributes.DeployCsss)
                    componentDefinition.DeployCss(deployCss.CssSelector, deployCss.CssStyle);

            if (attributes.DeployFunction != null)
                componentDefinition.DeployFunction(
                    attributes.DeployFunction.ReturnType,
                    attributes.DeployFunction.FunctionName,
                    attributes.DeployFunction.Parameters,
                    attributes.DeployFunction.Body,
                    attributes.DeployFunction.IsPublic);
        }
    }
}
