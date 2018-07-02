using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class PageBuilder: IPageBuilder
    {
        // TODO: Data binding
        // TODO: Render styles to dynamic assets
        // TODO: Implement AssetDeployment
        // TODO: Populate layout regions with specifiic content
        // TODO: Do not write the same asset multiple times on the same page
        // TODO: Write dynamic assets to buffer and cache for reuse on each page rendering

        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;
        private readonly IPageDependenciesFactory _pageDependenciesFactory;
        private readonly IFluentBuilder _fluentBuilder;

        public PageBuilder(
            IRequestRouter requestRouter,
            INameManager nameManager,
            IPageDependenciesFactory pageDependenciesFactory,
            IFluentBuilder fluentBuilder)
        {
            _requestRouter = requestRouter;
            _nameManager = nameManager;
            _pageDependenciesFactory = pageDependenciesFactory;
            _fluentBuilder = fluentBuilder;
        }

        public IPageDefinition Page(Type declaringType, IPackage package)
        {
            return new PageDefinition(
                declaringType, 
                _requestRouter, 
                _nameManager,
                _fluentBuilder,
                _pageDependenciesFactory,
                package);
        }
    }
}
