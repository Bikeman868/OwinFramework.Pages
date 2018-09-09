using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataScopeRules : IDataScopeRules, IDebuggable
    {
        public string ElementName { get; set; }
        public IList<IDataScope> DataScopes { get; private set; }
        public IList<Tuple<IDataSupplier, IDataDependency>> SuppliedDependencies { get; private set; }
        public IList<IDataSupply> DataSupplies { get; private set; }

        private readonly IDataScopeFactory _dataScopeFactory;
        private readonly object _lock = new object();

        public DataScopeRules(
            IDataScopeFactory dataScopeFactory)
        {
            _dataScopeFactory = dataScopeFactory;

            DataScopes = new List<IDataScope>();
            SuppliedDependencies = new List<Tuple<IDataSupplier, IDataDependency>>();
            DataSupplies = new List<IDataSupply>();
        }

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            var debug = new DebugDataScopeRules
            {
                Instance = this,
                Name = "data scope rules for " + ElementName,
            };

            lock (_lock)
            {
                debug.Scopes = DataScopes
                    .Select(s => new DebugDataScope { DataType = s.DataType, ScopeName = s.ScopeName })
                    .ToList();

                debug.DataSupplies = SuppliedDependencies
                    .Select(suppliedDependency =>
                        new DebugSuppliedDependency
                        {
                            Supplier = suppliedDependency.Item1.GetDebugInfo<DebugDataSupplier>(),

                            DataTypeSupplied = suppliedDependency.Item2 == null 
                                ? null
                                : new DebugDataScope
                                    {
                                        DataType = suppliedDependency.Item2.DataType,
                                        ScopeName = suppliedDependency.Item2.ScopeName
                                    }
                        })
                    .ToList();
            }

            return debug as T;
        }

        public override string ToString()
        {
            var description = "data scope rules";

            if (!string.IsNullOrEmpty(ElementName))
                description += " (" + ElementName + ")";

            return description;
        }

        public void AddScope(Type type, string scopeName)
        {
            lock (_lock)
            {
                if (DataScopes.Any(s =>
                    (s.DataType == type) &&
                    (string.Equals(s.ScopeName, scopeName, StringComparison.InvariantCultureIgnoreCase))))
                    return;

                var dataScope = _dataScopeFactory.Create(type, scopeName);
                DataScopes.Add(dataScope);
            }
        }

        public void AddSupply(IDataSupply supply)
        {
            if (supply == null) return;

            lock (_lock)
            {
                DataSupplies.Add(supply);
            }
        }

        public void AddSupplier(
            IDataSupplier supplier, 
            IDataDependency dependencyToSupply)
        {
            if (supplier == null) throw new ArgumentNullException("supplier");
            if (dependencyToSupply == null) throw new ArgumentNullException("dependencyToSupply");

            lock (_lock)
            {
                var suppliedDependency = SuppliedDependencies
                    .FirstOrDefault(d => d.Item1 == supplier && d.Item2.Equals(dependencyToSupply));
                
                if (suppliedDependency != null)
                    return;

                SuppliedDependencies.Add(new Tuple<IDataSupplier, IDataDependency>(supplier, dependencyToSupply));
            }
        }
    }
}
