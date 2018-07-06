using System;
using System.Reflection;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// The data catalog is responsible for maintaining a set of data providers
    /// and using these to ensure that the data context for a request contains the
    /// data needed by the controls that are being rendered.
    /// </summary>
    public interface IDataCatalog
    {
        /// <summary>
        /// Adds a data provider to the data catalog
        /// </summary>
        /// <param name="dataSupplier">The data supplier to register</param>
        IDataCatalog Register(IDataSupplier dataSupplier);

        /// <summary>
        /// Adds a data provider to the data catalog
        /// </summary>
        /// <param name="dataSupplierType">The type of data provider to register</param>
        /// <param name="factoryFunc">A function that knows how to construct 
        /// the data providers in your application</param>
        IDataCatalog Register(Type dataSupplierType, Func<Type, object> factoryFunc);

        /// <summary>
        /// Scans an assembly, finds all of the data providers and registers them
        /// with the data catalog
        /// </summary>
        /// <param name="assembly">The type of data provider to register</param>
        /// <param name="factoryFunc">A function that knows how to construct 
        /// the data providers in your application</param>
        IDataCatalog Register(Assembly assembly, Func<Type, object> factoryFunc);

        /// <summary>
        /// Locates a data provider that can satisfy the dependency. Returns null
        /// if none can be found
        /// </summary>
        IDataSupplier FindSupplier(IDataDependency dependency);
    }
}
