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

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(layout, attributes);

            var layoutDefinition = new LayoutDefinition(layout, _nameManager, _htmlHelper, _fluentBuilder, package);
            Configure(layoutDefinition, attributes);

            return layoutDefinition;
        }

        private void Configure(ILayoutDefinition layoutDefinition, AttributeSet attributes)
        {
            // TODO: Check that these are not already handled by IElementConfiguror

            if (attributes.Style != null)
            {
                if (!string.IsNullOrEmpty(attributes.Style.CssStyle))
                    layoutDefinition.Style(attributes.Style.CssStyle);
            }

            if (attributes.ChildStyle != null)
            {
                if (!string.IsNullOrEmpty(attributes.ChildStyle.CssStyle))
                    layoutDefinition.NestedStyle(attributes.ChildStyle.CssStyle);
            }

            if (attributes.Container != null)
            {
                if (!string.IsNullOrEmpty(attributes.Container.Tag))
                    layoutDefinition.Tag(attributes.Container.Tag);

                if (attributes.Container.ClassNames != null && attributes.Container.ClassNames.Length > 0)
                    layoutDefinition.ClassNames(attributes.Container.ClassNames);
            }

            if (attributes.NeedsComponents != null)
            {
                foreach (var component in attributes.NeedsComponents)
                {
                    layoutDefinition.NeedsComponent(component.ComponentName);
                }
            }

            if (attributes.ChildContainer != null)
            {
                if (!string.IsNullOrEmpty(attributes.ChildContainer.Tag))
                    layoutDefinition.NestingTag(attributes.ChildContainer.Tag);

                if (attributes.ChildContainer.ClassNames != null && attributes.ChildContainer.ClassNames.Length > 0)
                    layoutDefinition.NestedClassNames(attributes.ChildContainer.ClassNames);
            }

            if (attributes.UsesRegions != null)
                foreach (var usesRegion in attributes.UsesRegions)
                    layoutDefinition.Region(usesRegion.RegionName, usesRegion.RegionElement);
        }
    }
}
