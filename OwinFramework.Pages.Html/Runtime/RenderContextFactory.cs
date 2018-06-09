using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class RenderContextFactory : IRenderContextFactory
    {
        private readonly IAssetManager _assetManager;
        private readonly IHtmlWriterFactory _htmlWriterFactory;

        public RenderContextFactory(
            IAssetManager assetManager,
            IHtmlWriterFactory htmlWriterFactory)
        {
            _assetManager = assetManager;
            _htmlWriterFactory = htmlWriterFactory;
        }

        IRenderContext IRenderContextFactory.Create()
        {
            return new RenderContext(_assetManager, _htmlWriterFactory.Create());
        }
    }
}
