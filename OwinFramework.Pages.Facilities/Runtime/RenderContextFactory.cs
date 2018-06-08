using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Runtime
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
