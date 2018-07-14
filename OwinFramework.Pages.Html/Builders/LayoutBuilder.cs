using System;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class LayoutBuilder : ILayoutBuilder
    {
        private readonly ILayoutDependenciesFactory _layoutDependenciesFactory;
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IElementConfiguror _elementConfiguror;

        public LayoutBuilder(
            ILayoutDependenciesFactory layoutDependenciesFactory,
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            IElementConfiguror elementConfiguror,
            IFluentBuilder fluentBuilder)
        {
            _layoutDependenciesFactory = layoutDependenciesFactory;
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _elementConfiguror = elementConfiguror;
            _fluentBuilder = fluentBuilder;
        }

        ILayoutDefinition ILayoutBuilder.BuildUpLayout(object layoutInstance, Type declaringType, IPackage package)
        {
            var layout = layoutInstance as Layout ?? new Layout(_layoutDependenciesFactory);
            if (declaringType == null) declaringType = (layoutInstance ?? layout).GetType();

            var layoutDefinition = new LayoutDefinition(layout, _nameManager, _htmlHelper, _fluentBuilder, package);

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(layoutDefinition, attributes);

            return layoutDefinition;
        }
    }
}
