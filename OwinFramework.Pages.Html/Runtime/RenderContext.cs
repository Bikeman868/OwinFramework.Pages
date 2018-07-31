using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Debug;
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
        public IDataContext Data { get; set; }

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
            DeleteDataContextTree();
            Html.Dispose();
        }

        public DebugRenderContext GetDebugInfo()
        {
            return new DebugRenderContext
            {
                Instance = this,
                Data = _dataContexts
                    .Select(kv => new KeyValuePair<int, DebugDataContext>(kv.Key, kv.Value.GetDebugInfo()))
                    .ToDictionary(kv => kv.Key, kv => kv.Value)
            };
        }

        public void AddDataContext(int id, IDataContext dataContext)
        {
            _dataContexts.Add(id, dataContext);
        }

        public void SelectDataContext(int id)
        {
            Data = GetDataContext(id);
        }

        public IDataContext GetDataContext(int id)
        {
            IDataContext data;
            if (_dataContexts.TryGetValue(id, out data))
                return data;
            throw new Exception("The render context does not contain a data context with ID=" + id);
        }

        public void DeleteDataContextTree()
        {
            Data = null;
            foreach (var context in _dataContexts.Values)
                context.Dispose();
            _dataContexts.Clear();
        }
    }
}
