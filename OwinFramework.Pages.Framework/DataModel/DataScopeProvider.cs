using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Utility;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataScopeProvider : IDataScopeProvider
    {
        /*******************************************************************
        * Injected dependencies satisfied by IoC
        *******************************************************************/

        private readonly IIdManager _idManager;
        private readonly IDataScopeFactory _dataScopeFactory;
        private readonly IDataCatalog _dataCatalog;
        private readonly IDataContextFactory _dataContextFactory;

        public DataScopeProvider(
            IIdManager idManager,
            IDataScopeFactory dataScopeFactory,
            IDataCatalog dataCatalog,
            IDataContextFactory dataContextFactory)
        {
            _idManager = idManager;
            _dataScopeFactory = dataScopeFactory;
            _dataCatalog = dataCatalog;
            _dataContextFactory = dataContextFactory;

            Id = idManager.GetUniqueId();
        }

        /*******************************************************************
        * Information for helping with debugging only
        *******************************************************************/

        public DebugDataScopeProvider GetDebugInfo(int parentDepth, int childDepth)
        {
            var debug = new DebugDataScopeProvider
            {
                Instance = this,
                Name = (_isInstance ? "Instance " : string.Empty) + "#" + Id + " - " + ElementName,
                Id = Id
            };

            if (parentDepth != 0 && !ReferenceEquals(_parent, null))
            {
                debug.Parent = _parent.GetDebugInfo(parentDepth - 1, 0);
            }

            if (childDepth != 0)
            {
                lock(_children)
                {
                    debug.Children = _children
                        .Select(c => c.GetDebugInfo(0, childDepth - 1))
                        .Cast<DebugInfo>()
                        .ToList();
                }
            }

            lock (_dataScopes)
            {
                debug.Scopes = _dataScopes
                    .Select(s => new DebugDataScope { DataType = s.DataType, ScopeName = s.ScopeName })
                    .ToList();
            }

            lock(_suppliedDependencies)
            {
                debug.DataSupplies = _suppliedDependencies
                    .Select(suppliedDependency =>
                        new DebugSuppliedDependency
                        {
                            Supplier = suppliedDependency.Supplier.GetDebugInfo<DebugDataSupplier>(),
                            Supply = suppliedDependency.Supply.GetDebugInfo<DebugDataSupply>(),

                            DataSupplied = suppliedDependency.DependencySupplied == null 
                                ? null
                                : new DebugDataScope
                                {
                                    DataType = suppliedDependency.DependencySupplied.DataType,
                                    ScopeName = suppliedDependency.DependencySupplied.ScopeName
                                },

                            DependentSupplies = suppliedDependency.DependentSupplies == null
                                ? null
                                : suppliedDependency.DependentSupplies.Select(s => s.GetDebugInfo<DebugDataSupply>()).ToList()
                        })
                    .ToList();
            }

            return debug;
        }

        public override string ToString()
        {
            var description = "data scope provider " + (_isInstance ? "instance " : string.Empty) + "#" + Id;
            if (!string.IsNullOrEmpty(ElementName))
                description += " (" + ElementName + ")";
            return description;
        }

        /*******************************************************************
        * These class members can be used to set up the scope provider
        * prior to initialization.
        *******************************************************************/

        public int Id { get; private set; }
        public string ElementName { get; set; }
        private readonly bool _isInstance;
        private readonly List<IDataScope> _dataScopes = new List<IDataScope>();
        private readonly List<SuppliedDependency> _suppliedDependencies = new List<SuppliedDependency>();

        public void AddScope(Type type, string scopeName)
        {
            lock (_dataScopes)
            {
                if (_dataScopes.Any(s =>
                    (s.DataType == type) &&
                    (string.Equals(s.ScopeName, scopeName, StringComparison.InvariantCultureIgnoreCase))))
                    return;

                var dataScope = _dataScopeFactory.Create(type, scopeName);
                _dataScopes.Add(dataScope);
            }
        }

        public void AddSupply(IDataSupply supply)
        {
            if (supply == null) return;

            lock (_suppliedDependencies)
            {
                _suppliedDependencies.Add(
                    new SuppliedDependency
                    {
                        Supply = supply
                    });
            }
        }

        public IDataSupply AddSupplier(
            IDataSupplier supplier, 
            IDataDependency dependencyToSupply)
        {
            if (supplier == null) throw new ArgumentNullException("supplier");
            if (dependencyToSupply == null) throw new ArgumentNullException("dependencyToSupply");

            SuppliedDependency suppliedDependency;

            lock(_suppliedDependencies)
            {
                suppliedDependency = _suppliedDependencies
                    .FirstOrDefault(d => d.Supplier == supplier && d.DependencySupplied.Equals(dependencyToSupply));
                
                if (suppliedDependency != null)
                    return suppliedDependency.Supply;

                suppliedDependency = new SuppliedDependency
                {
                    Supplier = supplier,
                    DependencySupplied = dependencyToSupply,
                    Supply = supplier.GetSupply(dependencyToSupply)
                };

                _suppliedDependencies.Add(suppliedDependency);
            }

            if (_isInitialized)
            {
                var dataConsumer = supplier as IDataConsumer;
                if (dataConsumer != null)
                    suppliedDependency.DependentSupplies = dataConsumer.AddDependenciesToScopeProvider(this);
            }

            return suppliedDependency.Supply;
        }

        private DataScopeProvider(
            DataScopeProvider parent,
            IIdManager idManager,
            IDataScopeFactory dataScopeFactory,
            IDataCatalog dataCatalog,
            IDataContextFactory dataContextFactory)
        {
            _idManager = idManager;
            _dataScopeFactory = dataScopeFactory;
            _dataCatalog = dataCatalog;
            _dataContextFactory = dataContextFactory;

            Id = idManager.GetUniqueId();
            _isInstance = true;

            _dataScopes = parent._dataScopes.ToList();
            _suppliedDependencies = parent._suppliedDependencies.ToList();
        }

        public IDataScopeProvider CreateInstance()
        {
            return new DataScopeProvider(
                this, 
                _idManager, 
                _dataScopeFactory, 
                _dataCatalog, 
                _dataContextFactory);
        }

        private class SuppliedDependency
        {
            public IDataSupplier Supplier;
            public IDataDependency DependencySupplied;
            public IDataSupply Supply;

            private IList<IDataSupply> _dependentSupplies;
            public IList<IDataSupply> DependentSupplies
            {
                get { return _dependentSupplies; }
                set
                {
                    _dependentSupplies = new List<IDataSupply>();
                    if (value != null)
                    {
                        foreach(var supply in value)
                        {
                            var s = supply;
                            if (!_dependentSupplies.Any(d => ReferenceEquals(d, s)))
                                _dependentSupplies.Add(supply);
                        }
                    }
                }
            }
        }

        /*******************************************************************
        * These class members must be used to initialize the scope provider
        * before it can resolve dependencies
        *******************************************************************/

        private readonly IList<IDataScopeProvider> _children = new List<IDataScopeProvider>();
        private IDataScopeProvider _parent;
        private bool _isInitialized;

        public IDataScopeProvider Parent { get { return _parent; } }

        public void Initialize(IDataScopeProvider parent)
        {
            if (_isInitialized)
                throw new InvalidOperationException(
                    "The data scope provider can only be initialized once");

            _parent = parent;

            if (parent != null)
                parent.AddChild(this);

            _isInitialized = true;

            int suppliedDependenciesCount;
            lock (_suppliedDependencies) suppliedDependenciesCount = _suppliedDependencies.Count;

            for (var i = 0; i < suppliedDependenciesCount; i++)
            {
                SuppliedDependency suppliedDependency;
                lock (_suppliedDependencies) suppliedDependency = _suppliedDependencies[i];

                var consumer = suppliedDependency.Supplier as IDataConsumer;
                if (consumer != null)
                    suppliedDependency.DependentSupplies = consumer.AddDependenciesToScopeProvider(this);
            }
        }

        public void AddChild(IDataScopeProvider child)
        {
            if (child != null)
                _children.Add(child);
        }

        /*******************************************************************
        * These class members can only be used after initialization 
        * to resolve data needs into data supplies
        *******************************************************************/

        public IDataSupply AddDependency(IDataDependency dependency)
        {
            if (!_isInitialized)
                throw new InvalidOperationException(
                    "You can not add dependencies to a data scope provider until after initialization");

            if (_parent != null && !IsInScope(dependency) && !HasSupplier(dependency))
                return _parent.AddDependency(dependency);

            lock (_suppliedDependencies)
            {
                var suppliedDependency = _suppliedDependencies.FirstOrDefault(s => !ReferenceEquals(s.DependencySupplied, null) && s.DependencySupplied.Equals(dependency));
                if (suppliedDependency != null)
                    return suppliedDependency.Supply;

                suppliedDependency = _suppliedDependencies.FirstOrDefault(d => !ReferenceEquals(d.Supplier, null) && d.Supplier.IsSupplierOf(dependency));
                if (suppliedDependency != null)
                {
                    AddSupplier(suppliedDependency.Supplier, dependency);
                    return suppliedDependency.Supply;
                }
            }

            var supplier = _dataCatalog.FindSupplier(dependency);

            if (supplier == null)
                throw new Exception("Data scope provider was asked to provide " +
                    dependency + " data but the data catalog does not have any "+
                    "suppliers for that kind of data");

            return AddSupplier(supplier, dependency);
        }

        public IList<IDataSupply> AddConsumer(IDataConsumer consumer)
        {
            if (!_isInitialized)
                throw new InvalidOperationException(
                    "You can not add consumers to a data scope provider until after initialization");

            return ReferenceEquals(consumer, null) 
                ? null 
                : consumer.AddDependenciesToScopeProvider(this);
        }

        private bool HasSupplier(IDataDependency dependency)
        {
            if (ReferenceEquals(dependency, null))
                throw new ArgumentNullException("dependency");

            lock (_suppliedDependencies)
                return _suppliedDependencies.Any(d => 
                        !ReferenceEquals(d.Supplier, null) && 
                        d.Supplier.IsSupplierOf(dependency));
        }

        private readonly List<IDataSupply> _dataSupplies = new List<IDataSupply>();
        private bool _dataSuppliesBuilt;
        private bool _suppliesData;

        private List<SuppliedDependency> GetSuppliedDependenciesOrdered()
        {
            Func<SuppliedDependency, SuppliedDependency, bool> isDependentOn = (d1, d2) =>
                {
                    if (d1.DependentSupplies == null || d1.DependentSupplies.Count == 0) return false;
                    if (ReferenceEquals(d2.Supply, null)) return false;
                    return d1.DependentSupplies.Any(s => ReferenceEquals(s, d2.Supply));
                };
            var listSorter = new DependencyListSorter<SuppliedDependency>();

            lock(_suppliedDependencies)
                return listSorter.Sort(_suppliedDependencies, isDependentOn);
        }

        private void CheckForDynamic(IEnumerable<SuppliedDependency> suppliedDependencies)
        {
            foreach (var supplier in suppliedDependencies)
            {
                var supply = supplier.Supply;
                if (supply == null || 
                    !supply.IsStatic || 
                    supplier.DependentSupplies == null ||
                    supplier.DependentSupplies.All(s => s.IsStatic)) continue;

                supply.IsStatic = false;
                foreach (var dynamicSupply in supplier.DependentSupplies.Where(s => !s.IsStatic))
                {
                    dynamicSupply.AddOnSupplyAction(renderContext =>
                    {
                        var dataContext = renderContext.GetDataContext(Id);
                        supply.Supply(renderContext, dataContext);
                    });
                }
            }
        }

        private bool BuildSupplyList()
        {
            if (_dataSuppliesBuilt) return _suppliesData;

            lock (_dataSupplies)
            {
                if (_dataSuppliesBuilt) return _suppliesData;

                var orderedSuppliers = GetSuppliedDependenciesOrdered();

                CheckForDynamic(orderedSuppliers);

                _suppliedDependencies.Clear();
                _suppliedDependencies.AddRange(orderedSuppliers);

                _dataSupplies.Clear();
                _dataSupplies.AddRange(_suppliedDependencies.Select(s => s.Supply));

                _suppliesData = _dataSupplies.Count > 0;
                _dataSuppliesBuilt = true;
            }

            return _suppliesData;
        }

        /*******************************************************************
        * These class members can only be used after initialization 
        * to build the data context for a request. You only need to call
        * this on the root scope provider, it will traverse the child tree
        *******************************************************************/

        public void SetupDataContext(IRenderContext renderContext)
        {
            if (renderContext.Data != null)
                throw new Exception("The data scope provider should be used to setup a new render context");

            BuildDataContextTree(renderContext, null);

            if (_suppliesData)
                renderContext.SelectDataContext(Id);
        }

        public void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            var dataContext = parentDataContext;

            if (BuildSupplyList())
            {
                if (ReferenceEquals(parentDataContext, null))
                {
#if TRACE
                    renderContext.Trace(() => "Data scope provider #" + Id + " is building the root data context");
#endif
                    dataContext = _dataContextFactory.Create(renderContext, this);
                }
                else
                {
#if TRACE
                    renderContext.Trace(() => "Data scope provider #" + Id + " is building a child data context");
#endif
                    dataContext = parentDataContext.CreateChild(this);
                }

                renderContext.AddDataContext(Id, dataContext);

                int dataSupplyCount;
                lock (_dataSupplies) dataSupplyCount = _dataSupplies.Count;

                for (var i = 0; i < dataSupplyCount; i++)
                {
                    IDataSupply dataSupply;
                    lock (_dataSupplies) dataSupply = _dataSupplies[i];
                    if (dataSupply.IsStatic)
                    {
#if TRACE
                        renderContext.Trace(() => "Data scope provider #" + Id + " adding " + dataSupply);
#endif
                        dataSupply.Supply(renderContext, dataContext);
                    }
                    #if TRACE
else
                    {
                        renderContext.Trace(() => "Data scope provider #" + Id + " skipping " + dataSupply);
                    }
#endif
                }
            }

            int childCount;
            lock (_children) childCount = _children.Count;

            renderContext.TraceIndent();
            for (var i = 0; i < childCount; i++)
            {
                IDataScopeProvider child;
                lock (_children) child = _children[i];
                child.BuildDataContextTree(renderContext, dataContext);
            }
            renderContext.TraceOutdent();
        }

        IDataContext IDataScopeProvider.SetDataContext(IRenderContext renderContext)
        {
            var result = renderContext.Data;
            if (BuildSupplyList())
            {
#if TRACE
                renderContext.Trace(() => "Data scope provider #" + Id + " is establishing its data context in the render context");
#endif
                renderContext.SelectDataContext(Id);
            }
            return result;
        }

        /*******************************************************************
        * These class members are called by the data context to add data
        * that is missing because the application developer forgot to
        * declare the dependency
        *******************************************************************/

        public bool IsInScope(IDataDependency dependency)
        {
            if (ReferenceEquals(dependency, null))
                throw new ArgumentNullException("dependency");

            lock (_dataScopes)
                return _dataScopes.Any(s => s.IsMatch(dependency));
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
#if TRACE
            renderContext.Trace(() => "Data scope provider #" + Id + " has been notified of a missing dependency on " + missingDependency);
#endif

            AddDependency(missingDependency);
            renderContext.DeleteDataContextTree();

            var root = (IDataScopeProvider)this;
            while (!ReferenceEquals(root.Parent, null))
                root = root.Parent;

            root.SetupDataContext(renderContext);
        }

    }
}
