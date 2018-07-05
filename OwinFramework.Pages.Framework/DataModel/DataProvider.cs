using System;
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
    public class DataProvider: IDataProvider, IDataConsumer
    {
        public string Name { get; set; }
        public IPackage Package { get; set; }

        readonly IDataConsumer _dataConsumer;

        protected DataProvider(IDataProviderDependenciesFactory dependencies)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all data providers in all applications that use
            // this framework!!
            _dataConsumer = dependencies.DataConsumerFactory.Create();
        }

        /// <summary>
        /// Overrider this method to let the dependency resolution system know whether
        /// your data provider can satisfy a dependecy of not
        /// </summary>
        public virtual bool CanSatisfy(IDataDependency dependency)
        {
            return false;
        }

        /// <summary>
        /// Override this method if your data provider can return different types of data
        /// </summary>
        public virtual void Satisfy(IRenderContext renderContext, IDataContext dataContext, IDataDependency dependency)
        {
            Satisfy(renderContext, dataContext);
        }

        /// <summary>
        /// Override this method if your data provider only provides one type of data
        /// </summary>
        public virtual void Satisfy(IRenderContext renderContext, IDataContext dataContext)
        {
            throw new NotImplementedException("Data providers must override one of the Satisfy() method overloads");
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

        void IDataConsumer.ResolveDependencies(IDataScopeProvider scopeProvider)
        {
            _dataConsumer.ResolveDependencies(scopeProvider);
        }

        void IDataConsumer.NeedsData<T>(string scopeName)
        {
            _dataConsumer.NeedsData<T>(scopeName);
        }

        void IDataConsumer.NeedsData(Type dataType, string scopeName)
        {
            _dataConsumer.NeedsData(dataType, scopeName);
        }

        void IDataConsumer.CanUseData<T>(string scopeName = null)
        {
            _dataConsumer.CanUseData<T>(scopeName);
        }

        void IDataConsumer.CanUseData(Type dataType, string scopeName)
        {
            _dataConsumer.CanUseData(dataType, scopeName);
        }

        void IDataConsumer.NeedsProvider(IDataProvider dataProvider, IDataDependency dependency)
        {
            _dataConsumer.NeedsProvider(dataProvider, dependency);
        }

        #endregion
    }
}
