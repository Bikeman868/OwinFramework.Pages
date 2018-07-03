using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    /// <summary>
    /// Data scope providers are pages or regions. They introduce a new
    /// set of scopes that are a type+name. When controls try to resolve
    /// data the data scope provider represents a place where the scoping
    /// rules change
    /// </summary>
    public class DataScopeProvider: IDataScopeProvider
    {
        private readonly IIdManager _idManager;
        private readonly IDataScopeFactory _dataScopeFactory;
        private readonly IDataProviderDefinitionFactory _dataProviderDefinitionFactory;
        private readonly IDataCatalog _dataCatalog;
        private readonly IDataContextFactory _dataContextFactory;

        /// <summary>
        /// When constructing a tree of data contexts for a request the tree is
        /// traversed using these collections of children
        /// </summary>
        private readonly IList<IDataScopeProvider> _children;

        /// <summary>
        /// These are the data providers that need to be executed to
        /// set up a data contrext for a request
        /// </summary>
        private readonly IList<IDataProviderDefinition> _dataProviderDefinitions;

        /// <summary>
        /// These are the scopes that this scope provider overrides. Any
        /// requests for data that match this scope will be handled here
        /// and otherwise passed up to the parent
        /// </summary>
        private readonly IList<IDataScope> _dataScopes;

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
            IDataProviderDefinitionFactory dataProviderDefinitionFactory,
            IDataCatalog dataCatalog,
            IDataContextFactory dataContextFactory)
        {
            _idManager = idManager;
            _dataScopeFactory = dataScopeFactory;
            _dataProviderDefinitionFactory = dataProviderDefinitionFactory;
            _dataCatalog = dataCatalog;
            _dataContextFactory = dataContextFactory;

            _dataScopes = new List<IDataScope>();
            _dataProviderDefinitions = new List<IDataProviderDefinition>();
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
            _dataProviderDefinitions = original._dataProviderDefinitions.ToList();
            _children = new List<IDataScopeProvider>();

            Id = _idManager.GetUniqueId();
        }

        public IDataScopeProvider Clone()
        {
            return new DataScopeProvider(this);
        }

        DebugDataScopeProvider IDataScopeProvider.GetDebugInfo(int parentDepth, int childDepth)
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
                Dependencies = _dataScopes.Select(
                    s => (s.ScopeName ?? "") + " " + (s.DataType == null ? "" : s.DataType.FullName))
                    .ToList(),
                DataProviders = _dataProviderDefinitions.Select(
                    dp => new DebugDataProvider 
                    { 
                        Name = dp.DataProvider.Name,
                        Instance = dp.DataProvider,
                        Package = dp.DataProvider.Package == null
                            ? null
                            : dp.DataProvider.Package.GetDebugInfo(),
                        Dependency = dp.Dependency == null 
                            ? null
                            : dp.Dependency.DataType.DisplayName() + dp.Dependency.ScopeName
                    }).ToList()
            };
        }


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

        #region Scopes that are handled

        public void AddScope(Type type, string scopeName)
        {
            AddScope(type, scopeName, false);
        }

        public void ElementIsProvider(Type type, string scopeName)
        {
            AddScope(type, scopeName, true);
        }

        public bool IsInScope(Type type, string scopeName)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return _dataScopes.Any(s => 
                (s.DataType == null || s.DataType == type) &&
                (string.IsNullOrEmpty(s.ScopeName) || 
                 string.IsNullOrEmpty(scopeName) || 
                 string.Equals(s.ScopeName, scopeName, StringComparison.InvariantCultureIgnoreCase)));
        }

        public void AddScope(Type type, string scopeName, bool providedByElement)
        {
            if (_dataScopes.Any(s =>
                (s.DataType == type) &&
                (string.Equals(s.ScopeName, scopeName, StringComparison.InvariantCultureIgnoreCase))))
                return;

            var dataScope = _dataScopeFactory.Create(type, scopeName);
            dataScope.IsProvidedByElement = providedByElement;
            _dataScopes.Add(dataScope);
        }

        #endregion

        #region Data providers

        public List<IDataProviderDefinition> DataProviders
        {
            get { lock(_dataProviderDefinitions) return _dataProviderDefinitions.ToList(); }
        }

        public void ResolveDataProviders(IList<IDataProviderDefinition> existingProviders)
        {
            foreach (var dataScope in _dataScopes.Where(s => !s.IsProvidedByElement))
            {
                foreach (var dependency in dataScope.Dependencies)
                {
                    EnsureDependency(dependency, existingProviders);
                }
            }

            lock (_dataProviderDefinitions)
            {
                if (_dataProviderDefinitions.Count > 0)
                    existingProviders = existingProviders.Concat(_dataProviderDefinitions).ToList();
            }

            foreach (var child in _children)
            {
                child.ResolveDataProviders(existingProviders);
            }
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
            Add(missingDependency);

            renderContext.DeleteDataContextTree();
            SetupDataContext(renderContext);
        }

        public bool CanSatisfyDependency(IDataDependency dependency)
        {
            if (_dataScopes != null &&
                _dataScopes.Any(s => s.IsMatch(dependency) && s.IsProvidedByElement))
                return true;

            if (_dataProviderDefinitions != null && 
                _dataProviderDefinitions.Any(dp => dp.DataProvider.CanSatisfy(dependency)))
                return true;

            return false;
        }

        public void Add(IDataDependency dependency)
        {
            if (CanSatisfyDependency(dependency))
                return;

            var existingDependencies = DataProviders;

            var parent = _parent;
            while (parent != null)
            {
                if (parent.CanSatisfyDependency(dependency))
                    return;

                existingDependencies.AddRange(parent.DataProviders);
                parent = parent.Parent;
            }

            EnsureDependency(dependency, existingDependencies);
        }

        public void Add(IDataProviderDefinition dataProviderDefinition)
        {
            if (_dataProviderDefinitions.Any(dp =>
                dp.DataProvider == dataProviderDefinition.DataProvider &&
                (dp.Dependency == null || dataProviderDefinition.Dependency == null || dp.Dependency.DataType == dataProviderDefinition.Dependency.DataType)))
                return;

            _dataProviderDefinitions.Add(dataProviderDefinition);
        }

        private void EnsureDependency(IDataDependency dependency, IList<IDataProviderDefinition> existingProviders)
        {
            var dataProviderRegistration = _dataCatalog.FindProvider(dependency);

            if (dataProviderRegistration == null)
                throw new Exception("No data providers found that can supply missing data for " + dependency.ScopeName + " " + dependency.DataType.DisplayName());

            EnsureDependency(dataProviderRegistration, dependency, existingProviders);
        }

        private void EnsureDependency(IDataProviderRegistration dataProviderRegistration, IDataDependency dependency, IList<IDataProviderDefinition> existingProviders)
        {
            if (dataProviderRegistration == null) return;

            if (existingProviders.Any(p => p.DataProvider == dataProviderRegistration.DataProvider))
                return;

            Add(_dataProviderDefinitionFactory.Create(dataProviderRegistration.DataProvider, dependency));

            if (dataProviderRegistration.DependentProviders != null)
            {
                foreach (var dependent in dataProviderRegistration.DependentProviders)
                {
                    if (existingProviders.All(p => p.DataProvider != dataProviderRegistration.DataProvider))
                        Add(_dataProviderDefinitionFactory.Create(dependent));
                }
            }
        }

        #endregion

        #region Setup data context

        public void SetupDataContext(IRenderContext renderContext)
        {
            if (renderContext.Data != null)
                throw new Exception("The data scope provider should be used to setup a new render context");

            BuildDataContextTree(renderContext, null);

            renderContext.SelectDataContext(Id);
        }

        public void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            if ((_dataProviderDefinitions == null || _dataProviderDefinitions.Count == 0) &&
                (_children == null || _children.Count == 0))
                return;

            var dataContext = parentDataContext == null 
                ? _dataContextFactory.Create(renderContext, this) 
                : parentDataContext.CreateChild(this);

            renderContext.AddDataContext(Id, dataContext);

            if (_dataProviderDefinitions != null)
            {
                foreach (var providerDefinition in _dataProviderDefinitions)
                    providerDefinition.Execute(renderContext, dataContext);
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
    }
}
