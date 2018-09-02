using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    /// <summary>
    /// This is a mixin that should be added to any object that can
    /// consume data from one or more data suppliers
    /// </summary>
    public class DataConsumer: IDataConsumer, IDebuggable, IDataConsumerNeeds
    {
        private readonly IDataDependencyFactory _dataDependencyFactory;

        public List<Tuple<IDataProvider, IDataDependency>> DataProviderDependencies { get; private set; }
        public List<IDataSupply> DataSupplyDependencies { get; private set; }
        public List<IDataDependency> DataDependencies { get; private set; }

        IDataConsumerNeeds IDataConsumer.Needs { get { return this; } }

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
            if (DataDependencies == null)
                DataDependencies = new List<IDataDependency>();
            DataDependencies.Add(_dataDependencyFactory.Create(dataType, scopeName));
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

            if (DataProviderDependencies == null)
                DataProviderDependencies = new List<Tuple<IDataProvider, IDataDependency>>();

            DataProviderDependencies.Add(new Tuple<IDataProvider, IDataDependency>(dataProvider, dependency));
        }

        public void HasDependency(IDataSupply dataSupply)
        {
            if (DataSupplyDependencies == null)
                DataSupplyDependencies = new List<IDataSupply>();

            DataSupplyDependencies.Add(dataSupply);
        }

        DebugInfo IDebuggable.GetDebugInfo(int patentDepth, int childDepth)
        {
            return new DebugDataConsumer
            {
                DependentProviders = ReferenceEquals(DataProviderDependencies, null)
                    ? null
                    : DataProviderDependencies.Select(s => new DebugDataProviderDependency 
                        {
                            DataProvider = s.Item1.GetDebugInfo<DebugDataProvider>(),
                            Data = s.Item2 == null ? null : new DebugDataScope
                            {
                                DataType = s.Item2.DataType,
                                ScopeName = s.Item2.ScopeName
                            }
                        }).ToList(),
                DependentSupplies = ReferenceEquals(DataSupplyDependencies, null)
                    ? null
                    : DataSupplyDependencies.Select(s => s.GetDebugInfo<DebugDataSupply>()).ToList(),
                DependentData = ReferenceEquals(DataDependencies, null)
                    ? null
                    : DataDependencies.Select(s => new DebugDataScope
                        {
                            DataType = s.DataType,
                            ScopeName = s.ScopeName
                        }).ToList()
            };
        }
    }
}
