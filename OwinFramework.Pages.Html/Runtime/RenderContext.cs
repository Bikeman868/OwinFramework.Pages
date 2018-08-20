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
        private readonly IStringBuilderFactory _stringBuilderFactory;
        private readonly Action<IOwinContext, Func<string>> _trace;
        private readonly IThreadSafeDictionary<int, IDataContext> _dataContexts;

        private bool _traceEnabled;
        private int _traceIndentiation;

        public RenderContext(
            IAssetManager assetManager,
            IHtmlWriter htmlWriter,
            IDictionaryFactory dictionaryFactory,
            IStringBuilderFactory stringBuilderFactory,
            Action<IOwinContext, Func<string>> trace)
        {
            _assetManager = assetManager;
            _stringBuilderFactory = stringBuilderFactory;
            _trace = trace;
            _dataContexts = dictionaryFactory.Create<int, IDataContext>();
            Html = htmlWriter;
        }

        public IRenderContext Initialize(IOwinContext context)
        {
            OwinContext = context;

            _traceEnabled = !ReferenceEquals(context.Request.Query["trace"], null);

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
            Trace(() => "Render context adding data context #" + id);
            _dataContexts.Add(id, dataContext);
        }

        public void SelectDataContext(int id)
        {
            Trace(() => "Render context switching to data context #" + id);
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
            Trace(() => "Render context deleting data context tree");

            Data = null;
            foreach (var context in _dataContexts.Values)
                context.Dispose();
            _dataContexts.Clear();
        }

        public void Trace(Func<string> messageFunc)
        {
            if (!_traceEnabled)
                return;

            OutputTrace(messageFunc());
        }

        public void Trace<T>(Func<T, string> messageFunc, T arg)
        {
            if (!_traceEnabled)
                return;

            OutputTrace(messageFunc(arg));
        }

        public void TraceIndent()
        {
            _traceIndentiation++;
        }

        public void TraceOutdent()
        {
            _traceIndentiation--;
        }

        private void OutputTrace(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            var prefix = "Pages:" + new string(' ', 1 + _traceIndentiation * 2);

            var lines = message.Replace('\r', '\n').Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var l = prefix + line;
                    _trace(OwinContext, () => l);
                }
            }
        }
    }
}
