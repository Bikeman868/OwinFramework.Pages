using System;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    /// <summary>
    /// Plug-in to the fluent builder for building pages.
    /// Uses the supplied Page or constructs a new Page instance.
    /// Returns a fluent interface for defining the page characteristics
    /// </summary>
    internal class PageBuilder : IPageBuilder
    {
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;
        private readonly IPageDependenciesFactory _pageDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;
        private readonly IFluentBuilder _fluentBuilder;

        public PageBuilder(
            IRequestRouter requestRouter,
            INameManager nameManager,
            IPageDependenciesFactory pageDependenciesFactory,
            IElementConfiguror elementConfiguror,
            IFluentBuilder fluentBuilder)
        {
            _requestRouter = requestRouter;
            _nameManager = nameManager;
            _pageDependenciesFactory = pageDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _fluentBuilder = fluentBuilder;
        }

        public IPageDefinition BuildUpPage(object pageInstance, Type declaringType, IPackage package)
        {
            var page = pageInstance as Page ?? new Page(_pageDependenciesFactory);
            if (declaringType == null) declaringType = (pageInstance ?? page).GetType();

            var pageDefinition = new PageDefinition(page, _requestRouter, _nameManager, _fluentBuilder, package, declaringType);

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(pageDefinition, attributes);

            return pageDefinition;
        }
    }
}
