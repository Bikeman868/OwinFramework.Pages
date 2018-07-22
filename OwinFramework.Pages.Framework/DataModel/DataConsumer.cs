﻿using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    /// <summary>
    /// This is a mixin that should be added to any object that can
    /// consume data from one or more data suppliers
    /// </summary>
    public class DataConsumer: IDataConsumer
    {
        private readonly IDataDependencyFactory _dataDependencyFactory;

        private List<DataProviderDependency> _dataProviderDependencies;
        private List<IDataSupply> _dataSupplyDependencies;
        private List<IDataDependency> _dataDependencies;

        public DataConsumer(
            IDataDependencyFactory dataDependencyFactory)
        {
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
            if (ReferenceEquals(dataProvider, null)) throw new ArgumentNullException("dataProvider");

            var dataSupplier = dataProvider as IDataSupplier;

            if (dataSupplier == null)
                throw new Exception("Data provider " + dataProvider.Name + " is not a supplier of data");

            if (ReferenceEquals(dependency, null))
                dependency = dataSupplier.DefaultDependency;

            if (!dataSupplier.IsSupplierOf(dependency))
                throw new Exception("Data provider " + dataProvider.Name + " is not a supplier of " + dependency);

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

        public void AddDependenciesToScopeProvider(IDataScopeProvider dataScope)
        {
            if (_dataSupplyDependencies != null)
            {
                foreach (var dataSupply in _dataSupplyDependencies)
                    dataScope.AddSupply(dataSupply);
            }

            if (_dataProviderDependencies != null)
            {
                foreach (var dataProviderDependency in _dataProviderDependencies)
                {
                    var dataSupplier = dataProviderDependency.DataProvider as IDataSupplier;
                    if (!ReferenceEquals(dataSupplier, null))
                    {
                        dataScope.AddSupplier(dataSupplier, dataProviderDependency.Dependency);
                    }

                    var dataConsumer = dataProviderDependency.DataProvider as IDataConsumer;
                    if (!ReferenceEquals(dataConsumer, null))
                    {
                        dataConsumer.AddDependenciesToScopeProvider(dataScope);
                    }
                }
            }

            if (_dataDependencies != null)
            {
                foreach (var dependency in _dataDependencies)
                {
                    dataScope.AddDependency(dependency);
                }
            }
        }

        List<string> IDataConsumer.GetDebugDescription()
        {
            var description = new List<string>();

            if (_dataProviderDependencies != null)
                foreach (var dataProvider in _dataProviderDependencies)
                    description.Add("Needs " + dataProvider.Dependency + " from " + dataProvider.DataProvider);

            if (_dataSupplyDependencies != null)
                foreach (var dataSupply in _dataSupplyDependencies)
                    description.Add("Needs supply " + dataSupply.GetType().DisplayName());

            if (_dataDependencies != null)
                foreach (var dependency in _dataDependencies)
                    description.Add("Needs data " + dependency);
            
            return description;
        }

        private class DataProviderDependency
        {
            public IDataProvider DataProvider;
            public IDataDependency Dependency;
        }

    }
}
