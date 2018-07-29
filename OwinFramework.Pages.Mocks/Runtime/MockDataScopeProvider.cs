using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Mocks.Runtime
{
    public class MockDataScopeProvider: ConcreteImplementationProvider<IDataScopeProvider>, IDataScopeProvider
    {
        public int Id { get; set; }
        public string ElementName { get; set; }
        public IDataScopeProvider Parent { get; set; }
        public Type ScopeType { get; set; }
        public string ScopeName { get; set; }

        public Action<IDataContext> SupplyAction;
        public List<IDataDependency> DataDependencies = new List<IDataDependency>();
        public List<IDataConsumer> DataConsumers = new List<IDataConsumer>();
        public List<IDataSupply> DataSupplies = new List<IDataSupply>();

        public MockDataScopeProvider()
        {
            Id = 1;
        }

        protected override IDataScopeProvider GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public DebugDataScopeProvider GetDebugInfo(int parentDepth, int childDepth)
        {
            return new DebugDataScopeProvider
            {
                Name = "Mock data scope provider"
            };
        }

        public IDataScopeProvider Clone()
        {
            return this;
        }

        public void AddChild(IDataScopeProvider child)
        {
        }

        public void Initialize(IDataScopeProvider parent)
        {
            Parent = parent;
        }

        public void AddScope(Type type, string scopeName)
        {
            ScopeType = type;
            ScopeName = scopeName;
        }

        public IDataSupply AddSupplier(IDataSupplier supplier, IDataDependency dependency, IList<IDataSupply> supplyDependencies)
        {
            return new MockDataSupply();
        }

        public void AddSupply(IDataSupply supply)
        {
            DataSupplies.Add(supply);
        }

        public IDataSupply AddDependency(IDataDependency dependency)
        {
            DataDependencies.Add(dependency);
            return new MockDataSupply();
        }

        public IList<IDataSupply> AddConsumer(IDataConsumer consumer)
        {
            DataConsumers.Add(consumer);
            return null;
        }

        public bool IsInScope(IDataDependency dependency)
        {
            if (dependency == null) return false;
            if (dependency.DataType != ScopeType) return false;
            if (string.IsNullOrEmpty(dependency.ScopeName) || string.IsNullOrEmpty(ScopeName)) return true;
            return string.Equals(dependency.ScopeName, ScopeName, StringComparison.OrdinalIgnoreCase);
        }

        public IDataScopeProvider CreateInstance()
        {
            return this;
        }

        public void SetupDataContext(IRenderContext renderContext)
        {
        }

        public void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
        }

        private class MockDataSupply : IDataSupply
        {
            public Action<IDataContext> SupplyAction;
            public bool IsStatic { get { return true; } }
            public event EventHandler<DataSuppliedEventArgs> OnDataSupplied;

            public void Supply(IRenderContext renderContext, IDataContext dataContext)
            {
                if (SupplyAction != null)
                {
                    SupplyAction(dataContext);
                    if (OnDataSupplied != null)
                    {
                        var args = new DataSuppliedEventArgs 
                        { 
                            RenderContext = renderContext,
                            DataContext = dataContext
                        };
                        OnDataSupplied(this, args);
                    }
                }
            }
        }
    }
}
