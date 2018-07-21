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

            var regionDefinition = new RegionDefinition(region, _nameManager, _htmlHelper, _fluentBuilder, _regionDependenciesFactory.DataDependencyFactory, package);

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(regionDefinition, attributes);

            return regionDefinition;
        }
    }
}
