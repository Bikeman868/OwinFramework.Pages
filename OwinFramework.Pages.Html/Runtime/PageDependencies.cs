using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Extensions;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class PageDependencies: IPageDependencies
    {
        public IRenderContext RenderContext { get; private set; }
        public IDataContext DataContext { get; private set; }

        public PageDependencies(
            IRenderContext renderContext,
            IDataContext dataContext)
        {
            RenderContext = renderContext;
            DataContext = dataContext;
        }

        public IPageDependencies Initialize(IOwinContext context)
        {
            DataContext.Initialize(context);
            RenderContext.Initialize(context);
            return this;
        }

        public void Dispose()
        {
            DataContext.Dispose();
            RenderContext.Dispose();
        }
    }
}
