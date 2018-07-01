using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataConsumer: IDataConsumer
    {
        private readonly IDataProviderDefinitionFactory _dataProviderDefinitionFactory;
        private readonly IDataDependencyFactory _dataDependencyFactory;

        private List<IDataProviderDefinition> _dataProviders;
        private List<IDataDependency> _dataDependencies;

        public DataConsumer(
            IDataProviderDefinitionFactory dataProviderDefinitionFactory,
            IDataDependencyFactory dataDependencyFactory)
        {
            _dataProviderDefinitionFactory = dataProviderDefinitionFactory;
            _dataDependencyFactory = dataDependencyFactory;
        }

        public void NeedsData<T>(string scopeName)
        {
            NeedsData(typeof(T), scopeName);
        }

        public void NeedsData(Type dataType, string scopeName)
        {
            if (_dataDependencies == null)
                _dataDependencies = new List<IDataDependency>();
            _dataDependencies.Add(_dataDependencyFactory.Create(dataType, scopeName));
        }

        public void CanUseData<T>(string scopeName)
        {
            // In this case allow dependencies to be dynamically
            // discovered in response to requests for data
        }

        public void CanUseData(Type dataType, string scopeName)
        {
            // In this case allow dependencies to be dynamically
            // discovered in response to requests for data
        }

        public void NeedsProvider(IDataProvider dataProvider, IDataDependency dependency)
        {
            if (_dataProviders == null)
                _dataProviders = new List<IDataProviderDefinition>();
            _dataProviders.Add(_dataProviderDefinitionFactory.Create(dataProvider, dependency));
        }

        public void ResolveDependencies(IDataScopeProvider scopeProvider)
        {
            if (_dataProviders != null)
                foreach(var provider in _dataProviders)
                    scopeProvider.Add(provider);

            if (_dataDependencies != null)
                foreach (var dependency in _dataDependencies)
                    scopeProvider.Add(dependency);
        }
    }
}
