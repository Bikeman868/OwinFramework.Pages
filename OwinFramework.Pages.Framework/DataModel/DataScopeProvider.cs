using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataScopeProvider: IDataScopeProvider
    {
        private readonly IDataScopeFactory _dataScopeFactory;
        private readonly IDataCatalog _dataCatalog;
        private readonly IDataContextFactory _dataContextFactory;

        public int Id { get; private set; }
        public IDataScopeProvider Parent { get; private set; }
        public IList<IDataScope> DataScopes { get; private set; }
        public IDataContextDefinition DataContextDefinition { get; private set; }

        private readonly List<DataScopeProvider> _children;
        public IList<IDataScopeProvider> Children 
        { 
            get 
            { 
                return _children
                    .Cast<IDataScopeProvider>()
                    .ToList(); 
            } 
        }

        public DataScopeProvider(
            IDataScopeProvider parent,
            IIdManager idManager,
            IDataScopeFactory dataScopeFactory,
            IDataCatalog dataCatalog,
            IDataContextFactory dataContextFactory)
        {
            _dataScopeFactory = dataScopeFactory;
            _dataCatalog = dataCatalog;
            _dataContextFactory = dataContextFactory;
            Id = idManager.GetUniqueId();
            Parent = parent;
            DataScopes = new List<IDataScope>();

            _children = new List<DataScopeProvider>();
        }

        public bool Provides(Type type, string scopeName)
        {
            // TODO: dont add the same scope twice
            var dataScope = _dataScopeFactory.Create(type, scopeName);
            DataScopes.Add(dataScope);
            return true;
        }

        public void ResolveDataScopes()
        {
            DataContextDefinition = new DataContextDefinition();
            foreach (var dataScope in DataScopes)
            {
                foreach(var dependency in dataScope.Dependencies)
                {
                    var dataProviderRegistration = _dataCatalog.FindProvider(dependency);
                    if (dataProviderRegistration != null)
                    {
                        // TODO: deal with data provider dependencies
                        DataContextDefinition.Add(dataProviderRegistration.DataProvider, dependency);
                    }
                }
            }
        }

        public void SetupDataContext(IRenderContext renderContext)
        {
            if (renderContext.CurrentDataContext != null)
                throw new Exception("The data scope provider should be used to setup a new render context");

            SetupDataContext(renderContext, _dataContextFactory.Create(renderContext), false);

            renderContext.SelectDataContext(Id);
        }

        private void SetupDataContext(IRenderContext renderContext, IDataContext dataContext, bool isParent)
        {
            if (DataContextDefinition != null &&
                DataContextDefinition.DataProviders != null &&
                DataContextDefinition.DataProviders.Count > 0)
            {
                if (isParent)
                    dataContext = dataContext.CreateChild();

                foreach (var provider in DataContextDefinition.DataProviders)
                    provider.Execute(renderContext, dataContext);

                renderContext.AddDataContext(Id, dataContext);
            }
            else
            {
                if (!isParent)
                    renderContext.AddDataContext(Id, dataContext);
            }

            foreach(var child in _children)
            {
                child.SetupDataContext(renderContext, dataContext, true);
            }
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
            var dataProviderRegistration = _dataCatalog.FindProvider(missingDependency);

            if (dataProviderRegistration == null)
                throw new Exception("No data providers found that can supply missing data");

            DataContextDefinition.Add(dataProviderRegistration.DataProvider, missingDependency);
        }
    }
}
