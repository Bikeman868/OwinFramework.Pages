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
    public class DataProvider: IDataProvider
    {
        public string Name { get; set; }
        public IPackage Package { get; set; }

        protected DataProvider(IDataProviderDependenciesFactory dependencies)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all data providers in all applications that use
            // this framework!!
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
            throw new NotImplementedException("Data providers must override one of the EstablishContext overloads");
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
    }
}
