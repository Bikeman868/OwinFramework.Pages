using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Mocks.Runtime
{
    public class MockRenderContext: ConcreteImplementationProvider<IRenderContext>, IRenderContext
    {
        public IOwinContext OwinContext { get; set; }
        public IHtmlWriter Html { get; set; }
        public string Language { get; set; }
        public bool IncludeComments { get; set; }
        public IDataContext Data { get; set; }

        public IDictionary<int, IDataContext> DataContexts = new Dictionary<int, IDataContext>();

        protected override IRenderContext GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public IRenderContext Initialize(IOwinContext context)
        {
            OwinContext = context;
            return this;
        }

        public bool HasInitializationArea { get; set; }

        public void EnsureInitializationArea()
        {
            if (!HasInitializationArea)
            {
                HasInitializationArea = true;
                Html.WriteScriptOpen();
            }
        }

        public void EndInitializationArea()
        {
            if (HasInitializationArea)
            {
                Html.WriteScriptClose();
            }
        }

        public DebugRenderContext GetDebugInfo()
        {
            return new DebugRenderContext
            {
                Name = "Mock render context",
                Instance = this
            };
        }

        public void AddDataContext(int id, IDataContext dataContext)
        {
            DataContexts[id] = dataContext;
        }

        public void SelectDataContext(int id)
        {
            Data = DataContexts[id];
        }

        public void DeleteDataContextTree()
        {
            DataContexts.Clear();
            Data = null;
        }

        public IDataContext GetDataContext(int id)
        {
            return DataContexts[id];
        }

        public void Trace(Func<string> messageFunc)
        {
        }

        public void Trace<T>(Func<T, string> messageFunc, T arg)
        {
        }

        public void TraceIndent()
        {
        }

        public void TraceOutdent()
        {
        }
    }
}
