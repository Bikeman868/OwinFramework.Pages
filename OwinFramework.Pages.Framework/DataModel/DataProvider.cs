using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    /// <summary>
    /// You can inherit from this base class to insulate your implementation from
    /// future additions to the IDataProvider interface
    /// </summary>
    public class DataProvider: IDataProvider, IDataConsumer, IDataSupplier
    {
        public string Name { get; set; }
        public IPackage Package { get; set; }

        readonly IDataConsumer _dataConsumer;
        readonly IDataSupplier _dataSupplier;

        protected DataProvider(IDataProviderDependenciesFactory dependencies)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all data providers in all applications that use
            // this framework!!
            _dataConsumer = dependencies.DataConsumerFactory.Create();
            _dataSupplier = dependencies.DataSupplierFactory.Create();
        }

        public DebugDataProvider GetDebugInfo()
        {
            return new DebugDataProvider
            {
                Name = Name,
                Instance = this,
                Package = Package == null ? null : Package.GetDebugInfo()
            };
        }

        #region IDataConsumer

        void IDataConsumer.HasDependency<T>(string scopeName)
        {
            _dataConsumer.HasDependency<T>(scopeName);
        }

        void IDataConsumer.HasDependency(Type dataType, string scopeName)
        {
            _dataConsumer.HasDependency(dataType, scopeName);
        }

        void IDataConsumer.CanUseData<T>(string scopeName = null)
        {
            _dataConsumer.CanUseData<T>(scopeName);
        }

        void IDataConsumer.CanUseData(Type dataType, string scopeName)
        {
            _dataConsumer.CanUseData(dataType, scopeName);
        }

        void IDataConsumer.HasDependency(IDataProvider dataProvider, IDataDependency dependency)
        {
            _dataConsumer.HasDependency(dataProvider, dependency);
        }

        void IDataConsumer.HasDependency(IDataSupply dataSupply)
        {
            _dataConsumer.HasDependency(dataSupply);
        }

        IList<IDataSupply> IDataConsumer.GetDependencies(IDataScopeProvider dataScope)
        {
            return _dataConsumer.GetDependencies(dataScope);
        }

        #endregion

        #region IDataSupplier

        IList<Type> IDataSupplier.SuppliedTypes
        {
            get { return _dataSupplier.SuppliedTypes; }
        }

        bool IDataSupplier.IsScoped
        {
            get { return _dataSupplier.IsScoped; }
        }

        void IDataSupplier.Add(IDataDependency dependency, Action<IRenderContext, IDataContext, IDataDependency> action)
        {
            _dataSupplier.Add(dependency, action);
        }

        bool IDataSupplier.CanSupply(IDataDependency dependency)
        {
            return _dataSupplier.CanSupply(dependency);
        }

        IDataSupply IDataSupplier.GetSupply(IDataDependency dependency, IList<IDataSupply> dependencies)
        {
            return _dataSupplier.GetSupply(dependency, dependencies);
        }

        #endregion
    }
}
