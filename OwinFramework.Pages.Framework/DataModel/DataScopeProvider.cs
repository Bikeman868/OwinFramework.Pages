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
                debug.Dependencies = _suppliedDependencies
                    .Select(sd =>
                        {
                            if (sd.Dependency == null)
                                return sd.Supplier.GetType().DisplayName();
                            var d = sd.Dependency;
                            return d.DataType.DisplayName() + (string.IsNullOrEmpty(d.ScopeName) ? "" : " in '" + d.ScopeName + "' scope");
                        })
                    .ToList();
            }

            lock(_dataSupplies)
            {
                debug.DataSupplies = _dataSupplies
                    .Select(d => d.GetType().DisplayName())
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
        private readonly IList<IDataSupply> _dataSupplies = new List<IDataSupply>();
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
            lock(_dataSupplies) _dataSupplies.Add(supply);
        }

        public void AddSupplier(IDataSupplier supplier, IDataDependency dependency)
        {
            lock(_suppliedDependencies)
            {
                if (_suppliedDependencies.Any(d => 
                    d.Supplier == supplier && 
                    d.Dependency.Equals(dependency)))
                    return;

                _suppliedDependencies.Add(new SuppliedDependency
                    {
                        Supplier = supplier,
                        Dependency = dependency
                    });
            }

            var supply = supplier.GetSupply(dependency);
            AddSupply(supply);
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
            _dataSupplies = original._dataSupplies.ToList();
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
            public IDataDependency Dependency;
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

        public void AddDependency(IDataDependency dependency)
        {
            if (!_isInitialized)
                throw new InvalidOperationException(
                    "You can not add dependencies to a data scope provider until after initialization");

            if (_parent != null && !IsInScope(dependency))
            {
                _parent.AddDependency(dependency);
                return;
            }

            lock (_suppliedDependencies)
                if (_suppliedDependencies.Any(d => d.Supplier.IsSupplierOf(dependency)))
                    return;

            var supplier = _dataCatalog.FindSupplier(dependency);

            if (supplier == null)
                throw new Exception("Data scope provider was asked to provide " +
                    dependency + " data but the data catalog does not have any "+
                    "suppliers for that kind of data");

            AddSupplier(supplier, dependency);
            AddConsumer(supplier as IDataConsumer);
        }

        public void AddConsumer(IDataConsumer consumer)
        {
            if (!_isInitialized)
                throw new InvalidOperationException(
                    "You can not add consumers to a data scope provider until after initialization");

            if (ReferenceEquals(consumer, null)) return;

            consumer.AddDependenciesToScopeProvider(this);
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
            lock (_dataSupplies)
            {
                if ((_dataSupplies == null || _dataSupplies.Count == 0) &&
                    (_children == null || _children.Count == 0))
                    return;
            }

            var dataContext = parentDataContext == null
                ? _dataContextFactory.Create(renderContext, this)
                : parentDataContext.CreateChild(this);

            renderContext.AddDataContext(Id, dataContext);

            if (_dataSupplies != null)
            {
                int dataSupplyCount;
                lock (_dataSupplies)
                    dataSupplyCount = _dataSupplies.Count;

                for (var i = 0; i < dataSupplyCount; i++)
                {
                    IDataSupply dataSupply;
                    lock (_dataSupplies) dataSupply = _dataSupplies[i];
                    dataSupply.Supply(renderContext, dataContext);
                }
            }

            int childCount;
            lock (_children)
                childCount = _children.Count;

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
                if (_dataScopes.Any(s => s.IsMatch(dependency)))
                    return true;

            lock (_suppliedDependencies)
                if (_suppliedDependencies.Any(d => d.Supplier.IsSupplierOf(dependency)))
                    return true;

            return false;
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
