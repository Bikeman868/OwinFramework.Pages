using System;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class RegionBuilder: IRegionBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IRegionDependenciesFactory _regionDependenciesFactory;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IElementConfiguror _elementConfiguror;

        public RegionBuilder(
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            IRegionDependenciesFactory regionDependenciesFactory,
            IElementConfiguror elementConfiguror,
            IFluentBuilder fluentBuilder)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _regionDependenciesFactory = regionDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _fluentBuilder = fluentBuilder;
        }

        IRegionDefinition IRegionBuilder.BuildUpRegion(object regionInstance, Type declaringType, IPackage package)
        {
            var region = regionInstance as Region ?? new Region(_regionDependenciesFactory);
            if (declaringType == null) declaringType = (regionInstance ?? region).GetType();

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(region, attributes);

            var regionDefinition = new RegionDefinition(region, _nameManager, _htmlHelper, _fluentBuilder, package);
            Configure(regionDefinition, attributes);

            return regionDefinition;
        }

        private void Configure(IRegionDefinition regionDefinition, AttributeSet attributes)
        {
            // TODO: Check that these are not already handled by IElementConfiguror

            if (attributes.Style != null)
            {
                if (!string.IsNullOrEmpty(attributes.Style.CssStyle))
                    regionDefinition.Style(attributes.Style.CssStyle);
            }

            if (attributes.Repeat != null)
                regionDefinition.ForEach(
                    attributes.Repeat.RepeatType,
                    attributes.Repeat.RepeatScope,
                    attributes.Repeat.Tag,
                    attributes.Repeat.Style,
                    attributes.Repeat.ListScope,
                    attributes.Repeat.ClassNames);

            if (attributes.Container != null)
            {
                if (!string.IsNullOrEmpty(attributes.Container.Tag))
                    regionDefinition.Tag(attributes.Container.Tag);

                if (attributes.Container.ClassNames != null && attributes.Container.ClassNames.Length > 0)
                    regionDefinition.ClassNames(attributes.Container.ClassNames);
            }

            if (attributes.NeedsComponents != null)
            {
                foreach (var component in attributes.NeedsComponents)
                    regionDefinition.NeedsComponent(component.ComponentName);
            }

            if (attributes.UsesLayouts != null)
                foreach (var usesLayout in attributes.UsesLayouts)
                    regionDefinition.Layout(usesLayout.LayoutName);

            if (attributes.UsesComponents != null)
                foreach (var usesComponent in attributes.UsesComponents)
                    regionDefinition.Component(usesComponent.ComponentName);
        }
    }
}
