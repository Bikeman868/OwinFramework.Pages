using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class RenderContextFactory : IRenderContextFactory
    {
        private readonly IHtmlWriterFactory _htmlWriterFactory;

        public RenderContextFactory(
            IHtmlWriterFactory htmlWriterFactory)
        {
            _htmlWriterFactory = htmlWriterFactory;
        }

        IRenderContext IRenderContextFactory.Create()
        {
            return new RenderContext(_htmlWriterFactory.Create());
        }
    }
}
