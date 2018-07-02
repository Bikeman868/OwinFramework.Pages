using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html.Builders
{
    // TODO: Data binding
    // TODO: Repeating content on binding to a list
    // TODO: Render styles to dynamic assets
    // TODO: Implement AssetDeployment

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

        IRegionDefinition IRegionBuilder.Region(IPackage package)
        {
            return new RegionDefinition(_nameManager, _htmlHelper, _fluentBuilder, _regionDependenciesFactory, package);
        }
    }
}
