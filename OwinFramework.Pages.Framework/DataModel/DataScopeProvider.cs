﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        /// The parent scope or null if this is the page/service
        /// </summary>
        private IDataScopeProvider _parent;
        public IDataScopeProvider Parent { get { return _parent; } }

        public DataScopeProvider(
            IIdManager idManager,
            IDataScopeFactory dataScopeFactory,
            IDataProviderDefinitionFactory dataProviderDefinitionFactory,
            IDataCatalog dataCatalog,
            IDataContextFactory dataContextFactory,
            IDataScopeProvider parent)
        {
            _dataScopeFactory = dataScopeFactory;
            _dataProviderDefinitionFactory = dataProviderDefinitionFactory;
            _dataCatalog = dataCatalog;
            _dataContextFactory = dataContextFactory;

            _dataScopes = new List<IDataScope>();
            _dataProviderDefinitions = new List<IDataProviderDefinition>();
            _children = new List<IDataScopeProvider>();

            Id = idManager.GetUniqueId();

            SetParent(parent);
        }

        #region Parent/child tree structure

        public void SetParent(IDataScopeProvider parent)
        {
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
            if (_dataScopes.Any(s => 
                (s.DataType == type) && 
                (string.Equals(s.ScopeName, scopeName, StringComparison.InvariantCultureIgnoreCase))))
                return;

            var dataScope = _dataScopeFactory.Create(type, scopeName);
            _dataScopes.Add(dataScope);
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

        #endregion

        #region Data providers

        public List<IDataProvider> DataProviders
        {
            get { return _dataProviderDefinitions.Select(dp => dp.DataProvider).ToList(); }
        }

        public void ResolveDataProviders(IList<IDataProvider> existingProviders)
        {
            foreach (var dataScope in _dataScopes)
            {
                foreach (var dependency in dataScope.Dependencies)
                {
                    EnsureDependency(dependency, existingProviders);
                }
            }

            if (_dataProviderDefinitions.Count > 0)
                existingProviders = existingProviders.Concat(_dataProviderDefinitions.Select(d => d.DataProvider)).ToList();

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

        public void Add(IDataDependency dependency)
        {
            var existingDependencies = DataProviders;

            var parent = _parent;
            while (parent != null)
            {
                existingDependencies.AddRange(parent.DataProviders);
                parent = parent.Parent;
            }

            EnsureDependency(dependency, existingDependencies);
        }

        public void Add(IDataProvider dataProvider, IDataDependency dependency)
        {
            if (_dataProviderDefinitions.Any(dp => 
                dp.DataProvider == dataProvider && 
                (dp.Dependency == null || dependency == null || dp.Dependency.DataType == dependency.DataType)))
                return;

            _dataProviderDefinitions.Add(_dataProviderDefinitionFactory.Create(dataProvider, dependency));
        }

        private void EnsureDependency(IDataDependency dependency, IList<IDataProvider> existingProviders)
        {
            var dataProviderRegistration = _dataCatalog.FindProvider(dependency);

            if (dataProviderRegistration == null)
                throw new Exception("No data providers found that can supply missing data for " + dependency.ScopeName + " " + dependency.DataType);

            EnsureDependency(dataProviderRegistration, dependency, existingProviders);
        }

        private void EnsureDependency(IDataProviderRegistration dataProviderRegistration, IDataDependency dependency, IList<IDataProvider> existingProviders)
        {
            if (dataProviderRegistration == null) return;

            if (existingProviders.Any(p => p == dataProviderRegistration.DataProvider))
                return;

            Add(dataProviderRegistration.DataProvider, dependency);

            if (dataProviderRegistration.DependentProviders != null)
            {
                foreach (var dependent in dataProviderRegistration.DependentProviders)
                {
                    if (existingProviders.All(p => p != dataProviderRegistration.DataProvider))
                        Add(dependent, null);
                }
            }
        }

        #endregion

        #region Setup data context

        public void SetupDataContext(IRenderContext renderContext)
        {
            if (renderContext.Data != null)
                throw new Exception("The data scope provider should be used to setup a new render context");

            var rootDataContext = _dataContextFactory.Create(renderContext, this);
            BuildDataContextTree(renderContext, rootDataContext, false);

            renderContext.SelectDataContext(Id);
        }

        public void BuildDataContextTree(IRenderContext renderContext, IDataContext dataContext, bool isParentDataContext)
        {
            if (_dataProviderDefinitions != null && _dataProviderDefinitions.Count > 0)
            {
                if (isParentDataContext)
                    dataContext = dataContext.CreateChild(this);

                foreach (var providerDefinition in _dataProviderDefinitions)
                    providerDefinition.Execute(renderContext, dataContext);

                renderContext.AddDataContext(Id, dataContext);
            }
            else
            {
                if (!isParentDataContext)
                    renderContext.AddDataContext(Id, dataContext);
            }

            foreach(var child in _children)
            {
                child.BuildDataContextTree(renderContext, dataContext, true);
            }
        }

        #endregion
    }
}