using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    /// <summary>
    /// This is a mixin that should be added to any object that can
    /// consume data from one or more data providers
    /// </summary>
    public class DataConsumer: IDataConsumer
    {
        private readonly IDataSupplierFactory _dataProviderDefinitionFactory;
        private readonly IDataDependencyFactory _dataDependencyFactory;

        private List<DataProviderDependency> _dataProviderDependencies;
        private List<IDataSupply> _dataSupplyDependencies;
        private List<IDataDependency> _dataDependencies;

        public DataConsumer(
            IDataSupplierFactory dataProviderDefinitionFactory,
            IDataDependencyFactory dataDependencyFactory)
        {
            _dataProviderDefinitionFactory = dataProviderDefinitionFactory;
            _dataDependencyFactory = dataDependencyFactory;
        }

        public void HasDependency<T>(string scopeName)
        {
            HasDependency(typeof(T), scopeName);
        }

        public void HasDependency(Type dataType, string scopeName)
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

        public void HasDependency(IDataProvider dataProvider, IDataDependency dependency)
        {
            var dataSupplier = dataProvider as IDataSupplier;

            if (dataSupplier == null)
                throw new Exception("Data provider " + dataProvider.Name + " is not a supplier of data");

            if (dependency != null && !dataSupplier.IsSupplierOf(dependency))
                throw new Exception("Data provider " + dataProvider.Name + " does not supply the requested data");

            if (_dataProviderDependencies == null)
                _dataProviderDependencies = new List<DataProviderDependency>();

            _dataProviderDependencies.Add(new DataProviderDependency { DataProvider = dataProvider, Dependency = dependency });
        }

        public void HasDependency(IDataSupply dataSupply)
        {
            if (_dataSupplyDependencies == null)
                _dataSupplyDependencies = new List<IDataSupply>();

            _dataSupplyDependencies.Add(dataSupply);
        }

        public IList<IDataSupply> GetDependencies(IDataScopeProvider dataScope)
        {
            var dataSupplies = new List<IDataSupply>();

            if (_dataSupplyDependencies != null)
                dataSupplies.AddRange(_dataSupplyDependencies);

            if (_dataProviderDependencies != null)
            {
                foreach (var dataProviderDependency in _dataProviderDependencies)
                {
                    var dataConsumer = dataProviderDependency.DataProvider as IDataConsumer;
                    var dependencies = dataConsumer == null
                        ? null 
                        : dataConsumer.GetDependencies(dataScope);

                    var dataSupplier = dataProviderDependency.DataProvider as IDataSupplier;
                    if (dataSupplier != null)
                    {
                        var dataSupply = dataSupplier.GetSupply(dataProviderDependency.Dependency, dependencies);
                        dataSupplies.Add(dataSupply);
                    }
                }
            }

            if (_dataDependencies != null)
            {
                foreach (var dependency in _dataDependencies)
                {
                    var dataSupply = dataScope.Add(dependency);
                    dataSupplies.Add(dataSupply);
                }
            }

            return dataSupplies;
        }

        private class DataProviderDependency
        {
            public IDataProvider DataProvider;
            public IDataDependency Dependency;
        }


    }
}
