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
    public class MockDataContext : ConcreteImplementationProvider<IDataContext>, IDataContext
    {
        public IDataScopeProvider Scope { get; set; }

        public Dictionary<Type, object> Data = new Dictionary<Type, object>();

        protected override IDataContext GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public void Dispose()
        {
        }

        public DebugDataContext GetDebugInfo()
        {
            return new DebugDataContext
            {
                Name = "Mock data context",
                Instance = this
            };
        }

        public void Set<T>(T value, string scopeName = null, int level = 0)
        {
            var type = typeof(T);
            Data[type] = value;
        }

        public void Set(Type type, object value, string scopeName = null, int level = 0)
        {
            Data[type] = value;
        }

        public T Get<T>(string scopeName = null, bool required = true)
        {
            return (T)((IDataContext)this).Get(typeof(T), scopeName, required);
        }

        public object Get(Type type, string scopeName = null, bool required = true)
        {
            return Data[type];
        }

        public IDataContext CreateChild(IDataScopeProvider scopeProvider = null)
        {
            return this;
        }

    }
}
