using Microsoft.Owin;

namespace OwinFramework.Pages.Facilities.Runtime
{
    internal class PageDependenciesFactory: IPageDependenciesFactory
    {
        private readonly IRenderContextFactory _renderContextFactory;
        private readonly IDataContextFactory _dataContextFactory;

        public PageDependenciesFactory(
            IRenderContextFactory renderContextFactory,
            IDataContextFactory dataContextFactory)
        {
            _renderContextFactory = renderContextFactory;
            _dataContextFactory = dataContextFactory;
        }

        public IPageDependencies Create(IOwinContext context)
        {
            return new PageDependencies(
                _renderContextFactory.Create(),
                _dataContextFactory.Create())
                .Initialize(context);
        }
    }
}
