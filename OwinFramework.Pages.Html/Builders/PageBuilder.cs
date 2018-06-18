using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.Html.Runtime;

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

        public PageBuilder(
            IRequestRouter requestRouter,
            INameManager nameManager,
            IPageDependenciesFactory pageDependenciesFactory)
        {
            _requestRouter = requestRouter;
            _nameManager = nameManager;
            _pageDependenciesFactory = pageDependenciesFactory;
        }

        public IPageDefinition Page(Type declaringType)
        {
            return new PageDefinition(
                declaringType, 
                _requestRouter, 
                _nameManager,
                _pageDependenciesFactory);
        }
    }
}
