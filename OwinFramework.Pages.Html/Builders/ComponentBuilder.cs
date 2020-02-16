using System;
using System.Linq;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;
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
        private readonly IFrameworkConfiguration _frameworkConfiguration;

        public ComponentBuilder(
            INameManager nameManager,
            IAssetManager assetManager,
            IHtmlHelper htmlHelper,
            IComponentDependenciesFactory componentDependenciesFactory,
            IElementConfiguror elementConfiguror,
            IFrameworkConfiguration frameworkConfiguration,
            IFluentBuilder fluentBuilder)
        {
            _nameManager = nameManager;
            _assetManager = assetManager;
            _htmlHelper = htmlHelper;
            _componentDependenciesFactory = componentDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _fluentBuilder = fluentBuilder;
            _frameworkConfiguration = frameworkConfiguration;
        }

        IComponentDefinition IComponentBuilder.BuildUpComponent(object componentInstance, Type declaringType, IPackage package)
        {
            var component = componentInstance as Component ?? new Component(_componentDependenciesFactory);
            if (declaringType == null) declaringType = (componentInstance ?? component).GetType();

            var componentDefinition = new ComponentDefinition(component, _nameManager, _assetManager, _htmlHelper, _frameworkConfiguration, _fluentBuilder, package);
            var attributes = new AttributeSet(declaringType);

            _elementConfiguror.Configure(componentDefinition, attributes);

            return componentDefinition;
        }
    }
}
