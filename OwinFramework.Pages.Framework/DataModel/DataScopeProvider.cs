using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

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
                        .ToList();
                }
            }

            lock (_dataScopes)
            {
                debug.Scopes = _dataScopes
                    .Select(s => (s.ScopeName ?? "") + " " + (s.DataType == null ? "" : s.DataType.DisplayName()))
                    .ToList();
            }

            lock(_suppliedDependencies)
            {
                debug.DataSupplies = _suppliedDependencies
                    .Select(sd =>
                        {
                            var s = sd.Supplier;
                            if (ReferenceEquals(s, null))
                            {
                                return ReferenceEquals(sd.Supply, null) 
                                    ? null 
                                    : sd.Supply.GetType().DisplayName();
                            }
                            var d = sd.DependencySupplied;
                            var result = d.DataType.DisplayName();
                            if (!string.IsNullOrEmpty(d.ScopeName))
                                result += " in '" + d.ScopeName + "' scope";
                            result += " supplied by " + s.GetType().DisplayName();
                            return result;
                        })
                    .Where(s => s != null)
                    .ToList();
            }

            return debug;
        }

        /*******************************************************************
        * These class members can be used to set up the scope provider
        * prior to initialization.
        *******************************************************************/

        public int Id { get; private set; }
        public string ElementName { get; set; }
        private readonly bool _isInstance;
        private readonly IList<IDataScope> _dataScopes = new List<IDataScope>();
        private readonly IList<SuppliedDependency> _suppliedDependencies = new List<SuppliedDependency>();

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

        public IDataSupply AddSupplier(IDataSupplier supplier, IDataDependency dependency)
        {
            if (supplier == null) throw new ArgumentNullException("Supplier can not be null");
            if (dependency == null) throw new ArgumentNullException("Dependency can not be null");

            SuppliedDependency suppliedDependency;

            lock(_suppliedDependencies)
            {
                suppliedDependency = _suppliedDependencies
                    .FirstOrDefault(d => d.Supplier == supplier && d.DependencySupplied.Equals(dependency));
                
                if (suppliedDependency != null)
                    return suppliedDependency.Supply;

                suppliedDependency = new SuppliedDependency
                    {
                        Supplier = supplier,
                        DependencySupplied = dependency,
                        Supply = supplier.GetSupply(dependency)
                    };

                _suppliedDependencies.Add(suppliedDependency);
            }

            AddSupply(suppliedDependency.Supply);

            return suppliedDependency.Supply;
        }

        private DataScopeProvider(
            DataScopeProvider original,
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

            _dataScopes = original._dataScopes.ToList();
            _suppliedDependencies = original._suppliedDependencies.ToList();
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
            public IList<IDataSupply> DependentSupplies;
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

            var supply = AddSupplier(supplier, dependency);
            AddConsumer(supplier as IDataConsumer);

            return supply;
        }

        public void AddConsumer(IDataConsumer consumer)
        {
            if (!_isInitialized)
                throw new InvalidOperationException(
                    "You can not add consumers to a data scope provider until after initialization");

            if (ReferenceEquals(consumer, null)) return;

            var dependentSupplies = consumer.AddDependenciesToScopeProvider(this);
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

        private void OrderSuppliedDependencies()
        {
            Func<int, bool> canSwap = i =>
                {
                    if (i >= _suppliedDependencies.Count - 1) return false;

                    var sd = _suppliedDependencies[i + 1];
                    if (sd.DependentSupplies == null || sd.DependentSupplies.Count == 0) return true;

                    var supply = _suppliedDependencies[i].Supply;
                    return sd.DependentSupplies.Any(s => ReferenceEquals(s, supply));
                };

            Action<int> swap = i =>
                {
                    var t = _suppliedDependencies[i + 1];
                    _suppliedDependencies[i + 1] = _suppliedDependencies[i];
                    _suppliedDependencies[i] = t;
                };

            Func<int, bool> moveDown = null;
            moveDown = i =>
                {
                    var result = false;
                    while (canSwap(i)) 
                    {
                        swap(i);
                        i++;
                        result = true;
                    }
                    if (i < _suppliedDependencies.Count - 1)
                    {
                        if (moveDown(i+1))
                        {
                            while (canSwap(i))
                            {
                                swap(i);
                                i++;
                                result = true;
                            }
                        }
                    }
                    return result;
                };

            lock(_suppliedDependencies)
            {
                for (var i = 0; i < _suppliedDependencies.Count - 1; i++)
                {
                    moveDown(i);
                }
            }
        }

        private bool BuildSupplyList()
        {
            if (_dataSuppliesBuilt) return _suppliesData;

            OrderSuppliedDependencies();

            lock(_dataSupplies)
            {
                if (_dataSuppliesBuilt) return _suppliesData;

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

            renderContext.SelectDataContext(Id);
        }

        public void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            var dataContext = parentDataContext;

            if (BuildSupplyList())
            {
                dataContext = parentDataContext == null
                    ? _dataContextFactory.Create(renderContext, this)
                    : parentDataContext.CreateChild(this);

                renderContext.AddDataContext(Id, dataContext);

                int dataSupplyCount;
                lock (_dataSupplies) dataSupplyCount = _dataSupplies.Count;

                for (var i = 0; i < dataSupplyCount; i++)
                {
                    IDataSupply dataSupply;
                    lock (_dataSupplies) dataSupply = _dataSupplies[i];
                    if (dataSupply.IsStatic)
                        dataSupply.Supply(renderContext, dataContext);
                }
            }

            int childCount;
            lock (_children) childCount = _children.Count;

            for (var i = 0; i < childCount; i++)
            {
                IDataScopeProvider child;
                lock (_children) child = _children[i];
                child.BuildDataContextTree(renderContext, dataContext);
            }
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
            AddDependency(missingDependency);
            renderContext.DeleteDataContextTree();

            var root = (IDataScopeProvider)this;
            while (!ReferenceEquals(root.Parent, null))
                root = root.Parent;

            root.SetupDataContext(renderContext);
        }

    }
}
