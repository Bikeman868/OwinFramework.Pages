using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class RenderContext: IRenderContext, IDisposable
    {
        public IOwinContext OwinContext { get; private set; }
        public IHtmlWriter Html { get; private set; }
        public string Language { get; private set; }
        public bool IncludeComments { get; private set; }
        public IDataContext CurrentDataContext { get; private set; }

        private readonly IAssetManager _assetManager;
        private readonly IThreadSafeDictionary<int, IDataContext> _dataContexts;

        public RenderContext(
            IAssetManager assetManager,
            IHtmlWriter htmlWriter,
            IDictionaryFactory dictionaryFactory)
        {
            _assetManager = assetManager;
            _dataContexts = dictionaryFactory.Create<int, IDataContext>();
            Html = htmlWriter;
        }

        public IRenderContext Initialize(IOwinContext context)
        {
            OwinContext = context;

            var acceptLanguage = context.Request.Headers["Accept-Language"];
            Language = _assetManager.GetSupportedLanguage(acceptLanguage);

            IncludeComments = Html.IncludeComments;

            context.Response.Headers["Content-Language"] = Language;

            return this;
        }

        public void Dispose()
        {
            Html.Dispose();
        }

        public void AddDataContext(int id, IDataContext dataContext)
        {
            _dataContexts.Add(id, dataContext);
        }

        public void SelectDataContext(int id)
        {
            CurrentDataContext = _dataContexts[id];
        }
    }
}
