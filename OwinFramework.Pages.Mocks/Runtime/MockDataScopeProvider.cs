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

        public void SetParent(IDataScopeProvider parent)
        {
            Parent = parent;
        }

        public void AddScope(Type type, string scopeName)
        {
            ScopeType = type;
            ScopeName = scopeName;
        }

        public void AddElementScope(Type type, string scopeName)
        {
            ScopeType = type;
            ScopeName = scopeName;
        }

        public IDataSupply Add(IDataDependency dependency)
        {
            ScopeType = dependency.DataType;
            ScopeName = dependency.ScopeName;

            return new MockDataSupply { SupplyAction = SupplyAction };
        }

        public bool IsInScope(IDataDependency dependency)
        {
            if (dependency == null) return false;
            if (dependency.DataType != ScopeType) return false;
            if (string.IsNullOrEmpty(dependency.ScopeName) || string.IsNullOrEmpty(ScopeName)) return true;
            return string.Equals(dependency.ScopeName, ScopeName, StringComparison.OrdinalIgnoreCase);
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

            public IList<IDataSupply> DependentSupplies { get { return null; } }

            public void Supply(IRenderContext renderContext, IDataContext dataContext)
            {
                if (SupplyAction != null)
                    SupplyAction(dataContext);
            }
        }
    }
}
