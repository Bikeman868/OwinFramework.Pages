using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Mocks.DataModel;

namespace OwinFramework.Pages.Mocks.Runtime
{
    public class MockDataScopeRules: ConcreteImplementationProvider<IDataScopeRules>, IDataScopeRules
    {
        public int Id { get; set; }
        public string ElementName { get; set; }
        public IDataScopeRules Parent { get; set; }
        public Type ScopeType { get; set; }
        public string ScopeName { get; set; }

        public Action<IDataContext> SupplyAction;
        public List<IDataDependency> DataDependencies = new List<IDataDependency>();
        public List<IDataConsumer> DataConsumers = new List<IDataConsumer>();
        public List<IDataSupply> DataSupplies = new List<IDataSupply>();

        public MockDataScopeRules()
        {
            Id = 1;
        }

        protected override IDataScopeRules GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public DebugDataScopeRules GetDebugInfo(int parentDepth, int childDepth)
        {
            return new DebugDataScopeRules
            {
                Name = "Mock data scope provider"
            };
        }

        public IDataScopeRules Clone()
        {
            return this;
        }

        public void AddChild(IDataScopeRules child)
        {
        }

        public void Initialize(IDataScopeRules parent)
        {
            Parent = parent;
        }

        public void AddScope(Type type, string scopeName)
        {
            ScopeType = type;
            ScopeName = scopeName;
        }

        public void AddSupplier(IDataSupplier supplier, IDataDependency dependency)
        {
        }

        public void AddSupply(IDataSupply supply)
        {
            DataSupplies.Add(supply);
        }

        public IDataSupply AddDependency(IDataDependency dependency)
        {
            DataDependencies.Add(dependency);
            return new MockDataSupplier.DataSupplier();
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

        public IDataScopeRules CreateInstance()
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

        public IDataContext SetDataContext(IRenderContext renderContext)
        {
            return renderContext.Data;
        }

        public IList<IDataScope> DataScopes
        {
            get { return new List<IDataScope>(); }
        }

        public IList<Tuple<IDataSupplier, IDataDependency>> SuppliedDependencies
        {
            get { return new List<Tuple<IDataSupplier, IDataDependency>>(); }
        }

        IList<IDataSupply> IDataScopeRules.DataSupplies
        {
            get { return new List<IDataSupply>(); }
        }
    }
}
