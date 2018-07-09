using System;
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

        public RegionBuilder(
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            IRegionDependenciesFactory regionDependenciesFactory,
            IFluentBuilder fluentBuilder)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _regionDependenciesFactory = regionDependenciesFactory;
            _fluentBuilder = fluentBuilder;
        }

        IRegionDefinition IRegionBuilder.BuildUpRegion(object regionInstance, Type declaringType, IPackage package)
        {
            var region = regionInstance as Region ?? new Region(_regionDependenciesFactory);
            if (declaringType == null) declaringType = (regionInstance ?? region).GetType();

            return new RegionDefinition(
                region,
                declaringType, 
                _nameManager, 
                _htmlHelper, 
                _fluentBuilder,
                package);
        }
    }
}
