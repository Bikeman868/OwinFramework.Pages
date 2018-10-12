using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
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
    public class DataProvider: IDataProvider, IDataConsumer, IDebuggable
    {
        public ElementType ElementType { get { return ElementType.DataProvider; } }

        public string Name { get; set; }
        public IPackage Package { get; set; }

        protected readonly IDataConsumer DataConsumer;
        protected readonly IDataSupplier DataSupplier;
        protected readonly IDataProviderDependenciesFactory Dependencies;

        public DataProvider(IDataProviderDependenciesFactory dependencies)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all data providers in all applications that use
            // this framework!!
            Dependencies = dependencies;
            DataConsumer = dependencies.DataConsumerFactory.Create();
            DataSupplier = dependencies.DataSupplierFactory.Create();
        }

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            if (typeof(T).IsAssignableFrom(typeof(DebugDataProvider)))
            {
                return new DebugDataProvider
                {
                    Name = Name,
                    Instance = this,
                    Package = Package.GetDebugInfo<DebugPackage>(),
                    DataConsumer = DataConsumer.GetDebugInfo<DebugDataConsumer>(),
                    DataSupplier = DataSupplier.GetDebugInfo<DebugDataSupplier>()
                } as T;
            }

            if (typeof(T).IsAssignableFrom(typeof(DebugDataConsumer)))
            {
                return DataConsumer.GetDebugInfo<T>(parentDepth, childDepth);
            }

            if (typeof(T).IsAssignableFrom(typeof(DebugDataSupplier)))
            {
                return DataSupplier.GetDebugInfo<T>(parentDepth, childDepth);
            }

            return null;
        }

        public override string ToString()
        {
            var result = "data provider '" + Name + "'";
            if (GetType() != typeof(DataProvider))
                result += " of type " + GetType().DisplayName();
            return result;
        }

        public void Add<T>(string scopeName = null)
        {
            var suppliedDependency = Dependencies.DataDependencyFactory.Create<T>(scopeName);
            DataSupplier.Add(suppliedDependency, Supply);
        }

        public void Add(Type type, string scopeName = null)
        {
            if (ReferenceEquals(type, null)) return;

            var suppliedDependency = Dependencies.DataDependencyFactory.Create(type, scopeName);
            DataSupplier.Add(suppliedDependency, Supply);
        }

        protected virtual void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
        }

        #region IDataConsumer  Mixin

        IDataConsumerNeeds IDataConsumer.GetConsumerNeeds()
        { 
            return DataConsumer.GetConsumerNeeds();
        }

        void IDataConsumer.HasDependency<T>(string scopeName)
        {
            DataConsumer.HasDependency<T>(scopeName);
        }

        void IDataConsumer.HasDependency(Type dataType, string scopeName)
        {
            DataConsumer.HasDependency(dataType, scopeName);
        }

        void IDataConsumer.CanUseData<T>(string scopeName = null)
        {
            DataConsumer.CanUseData<T>(scopeName);
        }

        void IDataConsumer.CanUseData(Type dataType, string scopeName)
        {
            DataConsumer.CanUseData(dataType, scopeName);
        }

        void IDataConsumer.HasDependency(IDataProvider dataProvider, IDataDependency dependency)
        {
            DataConsumer.HasDependency(dataProvider, dependency);
        }

        void IDataConsumer.HasDependency(IDataSupply dataSupply)
        {
            DataConsumer.HasDependency(dataSupply);
        }

        #endregion

        #region IDataSupplier Mixin

        IDataDependency IDataSupplier.DefaultDependency { get { return DataSupplier.DefaultDependency; } }
        IList<Type> IDataSupplier.SuppliedTypes { get { return DataSupplier.SuppliedTypes; } }

        bool IDataSupplier.CanSupplyScoped(Type type)
        { 
            return DataSupplier.CanSupplyScoped(type); 
        }

        bool IDataSupplier.CanSupplyUnscoped(Type type)
        {
            return DataSupplier.CanSupplyUnscoped(type);
        }

        void IDataSupplier.Add(IDataDependency dependency, Action<IRenderContext, IDataContext, IDataDependency> action)
        {
            DataSupplier.Add(dependency, action);
        }

        bool IDataSupplier.IsSupplierOf(IDataDependency dependency)
        {
            return DataSupplier.IsSupplierOf(dependency);
        }

        IDataSupply IDataSupplier.GetSupply(IDataDependency dependency)
        {
            return DataSupplier.GetSupply(dependency);
        }

        #endregion
    }
}
