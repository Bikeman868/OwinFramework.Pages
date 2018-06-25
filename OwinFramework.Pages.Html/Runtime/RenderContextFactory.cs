using OwinFramework.Pages.Core.Interfaces.Collections;
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
        private readonly IDictionaryFactory _dictionaryFactory;

        public RenderContextFactory(
            IAssetManager assetManager,
            IHtmlWriterFactory htmlWriterFactory,
            IHtmlConfiguration htmlConfiguration,
            IDictionaryFactory dictionaryFactory)
        {
            _assetManager = assetManager;
            _htmlWriterFactory = htmlWriterFactory;
            _htmlConfiguration = htmlConfiguration;
            _dictionaryFactory = dictionaryFactory;
        }

        IRenderContext IRenderContextFactory.Create()
        {
            var htmlWriter = _htmlWriterFactory.Create(
                    _htmlConfiguration.HtmlFormat,
                    _htmlConfiguration.Indented,
                    _htmlConfiguration.IncludeComments);

            return new RenderContext(
                _assetManager, 
                htmlWriter,
                _dictionaryFactory);
        }
    }
}
