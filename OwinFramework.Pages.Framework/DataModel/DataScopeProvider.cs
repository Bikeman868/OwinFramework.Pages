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
        private readonly IIdManager _idManager;
        private readonly IDataScopeFactory _dataScopeFactory;
        private readonly IDataSupplierFactory _dataProviderDefinitionFactory;
        private readonly IDataCatalog _dataCatalog;
        private readonly IDataContextFactory _dataContextFactory;

        /// <summary>
        /// When constructing a tree of data contexts for a request the tree is
        /// traversed using these collections of children
        /// </summary>
        private readonly IList<IDataScopeProvider> _children;

        /// <summary>
        /// These are the scopes that this scope provider overrides. Any
        /// requests for data that match this scope will be handled here
        /// and otherwise passed up to the parent
        /// </summary>
        private readonly IList<IDataScope> _dataScopes;

        /// <summary>
        /// A list of the dependencies that matched this scope. These are
        /// executed to build the data context tree for each request
        /// </summary>
        private readonly IList<DependencySupply> _dependencySupplies;

        /// <summary>
        /// This unique ID is used to index the data contexts in the render context
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Set this to the name of the element that this is providing data scope
        /// for. This will be either a region, page or service
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        /// The parent scope or null if this is the page/service
        /// </summary>
        private IDataScopeProvider _parent;

        public DataScopeProvider(
            IIdManager idManager,
            IDataScopeFactory dataScopeFactory,
            IDataSupplierFactory dataProviderDefinitionFactory,
            IDataCatalog dataCatalog,
            IDataContextFactory dataContextFactory)
        {
            _idManager = idManager;
            _dataScopeFactory = dataScopeFactory;
            _dataProviderDefinitionFactory = dataProviderDefinitionFactory;
            _dataCatalog = dataCatalog;
            _dataContextFactory = dataContextFactory;

            _dataScopes = new List<IDataScope>();
            _dependencySupplies = new List<DependencySupply>();
            _children = new List<IDataScopeProvider>();

            Id = idManager.GetUniqueId();
        }

        private DataScopeProvider(DataScopeProvider original)
        {
            _idManager = original._idManager;
            _dataScopeFactory = original._dataScopeFactory;
            _dataProviderDefinitionFactory = original._dataProviderDefinitionFactory;
            _dataCatalog = original._dataCatalog;
            _dataContextFactory = original._dataContextFactory;

            _dataScopes = original._dataScopes.ToList();
            _dependencySupplies = new List<DependencySupply>();
            _children = new List<IDataScopeProvider>();

            Id = _idManager.GetUniqueId();
        }

        public IDataScopeProvider Clone()
        {
            return new DataScopeProvider(this);
        }

        #region Debug info

        public DebugDataScopeProvider GetDebugInfo(int parentDepth, int childDepth)
        {
            return new DebugDataScopeProvider
            {
                Instance = this,
                Name = Id + " (" + ElementName + ")",
                Id = Id,
                Parent = _parent == null || parentDepth == 0
                    ? null
                    : _parent.GetDebugInfo(parentDepth - 1, 0),
                Children = _children == null || childDepth == 0
                    ? null
                    : _children.Select(c => c.GetDebugInfo(0, childDepth - 1)).ToList(),
                Scopes = _dataScopes.Select(
                    s => (s.ScopeName ?? "") + " " + (s.DataType == null ? "" : s.DataType.FullName))
                    .ToList(),
                Dependencies = _dependencySupplies.Select(
                    d => (string.IsNullOrEmpty(d.Dependency.ScopeName) ? string.Empty : d.Dependency.ScopeName + " ") + d.Dependency.DataType.DisplayName())
                    .ToList(),
            };
        }

        #endregion

        #region Parent/child tree structure

        public IDataScopeProvider Parent { get { return _parent; } }

        public void SetParent(IDataScopeProvider parent)
        {
            if (_parent != null)
                throw new InvalidOperationException(
                    "The parent of this data scope provider has already been set");

            _parent = parent;

            if (parent != null)
                parent.AddChild(this);
        }

        public void AddChild(IDataScopeProvider child)
        {
            if (child != null)
                _children.Add(child);
        }

        #endregion

        #region Scopes handled by this provider

        public void AddScope(Type type, string scopeName)
        {
            AddScope(type, scopeName, false);
        }

        public void AddElementScope(Type type, string scopeName)
        {
            AddScope(type, scopeName, true);
        }

        private void AddScope(Type type, string scopeName, bool providedByElement)
        {
            if (_dataScopes.Any(s =>
                (s.DataType == type) &&
                (string.Equals(s.ScopeName, scopeName, StringComparison.InvariantCultureIgnoreCase))))
                return;

            var dataScope = _dataScopeFactory.Create(type, scopeName);
            dataScope.IsProvidedByElement = providedByElement;
            _dataScopes.Add(dataScope);
        }

        public bool IsInScope(IDataDependency dependency)
        {
            if (dependency == null)
                throw new ArgumentNullException("dependency");

            return _dataScopes.Any(s => s.IsMatch(dependency));
        }

        #endregion

        #region Satisfying dependencies

        public IDataSupply Add(IDataDependency dependency)
        {
            var result = _dependencySupplies.FirstOrDefault(ds => ds.Dependency.Equals(dependency));
            if (result != null) return result.DataSupply;

            if (_dataScopes.Any(s => s.IsProvidedByElement && s.IsMatch(dependency)))
                return null;

            if (_parent != null && !IsInScope(dependency))
                return _parent.Add(dependency);

            var dataSupplier = _dataCatalog.FindSupplier(dependency);

            if (dataSupplier == null)
                throw new Exception(
                    "There are no registered data suppliers that can fulfil the dependency on " +
                    dependency.DataType.DisplayName() + (string.IsNullOrEmpty(dependency.ScopeName) 
                    ? string.Empty : " with '" + dependency.ScopeName + "' scope"));
            
            IList<IDataSupply> supplierDependencies = null;
            var dataConsumer = dataSupplier as IDataConsumer;
            if (dataConsumer != null)
                supplierDependencies = dataConsumer.GetDependencies(this);

            var supply = dataSupplier.GetSupply(dependency, supplierDependencies);

            var dependencySupply = new DependencySupply
            {
                Dependency = dependency,
                DataSupply = supply
            };
            _dependencySupplies.Add(dependencySupply);

            return supply;
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
            Add(missingDependency);

            renderContext.DeleteDataContextTree();
            SetupDataContext(renderContext);
        }

        #endregion

        #region Setting up the data context

        public void SetupDataContext(IRenderContext renderContext)
        {
            if (renderContext.Data != null)
                throw new Exception("The data scope provider should be used to setup a new render context");

            BuildDataContextTree(renderContext, null);

            renderContext.SelectDataContext(Id);
        }

        public void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            lock (_dependencySupplies)
            {
                if ((_dependencySupplies == null || _dependencySupplies.Count == 0) &&
                    (_children == null || _children.Count == 0))
                    return;
            }

            var dataContext = parentDataContext == null
                ? _dataContextFactory.Create(renderContext, this)
                : parentDataContext.CreateChild(this);

            renderContext.AddDataContext(Id, dataContext);

            if (_dependencySupplies != null)
            {
                int count;
                lock (_dependencySupplies)
                    count = _dependencySupplies.Count;

                for (var i = 0; i < count; i++)
                {
                    DependencySupply dependencySupply;
                    lock (_dependencySupplies)
                        dependencySupply = _dependencySupplies[i];

                    dependencySupply.DataSupply.Supply(renderContext, dataContext);
                }
            }

            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.BuildDataContextTree(renderContext, dataContext);
                }
            }

        }

        #endregion
    
        private class DependencySupply
        {
            public IDataDependency Dependency;
            public IDataSupply DataSupply;
        }
    }
}
