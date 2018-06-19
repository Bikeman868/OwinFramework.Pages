using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class RenderContextFactory : IRenderContextFactory
    {
        private readonly IAssetManager _assetManager;
        private readonly IHtmlWriterFactory _htmlWriterFactory;
        private readonly IHtmlConfiguration _htmlConfiguration;

        public RenderContextFactory(
            IAssetManager assetManager,
            IHtmlWriterFactory htmlWriterFactory,
            IHtmlConfiguration htmlConfiguration)
        {
            _assetManager = assetManager;
            _htmlWriterFactory = htmlWriterFactory;
            _htmlConfiguration = htmlConfiguration;
        }

        IRenderContext IRenderContextFactory.Create()
        {
            return new RenderContext(
                _assetManager, 
                _htmlWriterFactory.Create(
                    _htmlConfiguration.HtmlFormat,
                    _htmlConfiguration.Indented,
                    _htmlConfiguration.IncludeComments));
        }
    }
}
