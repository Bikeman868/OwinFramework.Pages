using System;
using System.Reflection;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Managers
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
        /// <param name="dataProvider">The data provider to register</param>
        IDataCatalog Register(IDataProvider dataProvider);

        /// <summary>
        /// Adds a data provider to the data catalog
        /// </summary>
        /// <param name="dataProviderType">The type of data provider to register</param>
        /// <param name="factoryFunc">A function that knows how to construct 
        /// the data providers in your application</param>
        IDataCatalog Register(Type dataProviderType, Func<Type, object> factoryFunc);

        /// <summary>
        /// Scans an assembly, finds all of the data providers and registers them
        /// with the data catalog
        /// </summary>
        /// <param name="assembly">The type of data provider to register</param>
        /// <param name="factoryFunc">A function that knows how to construct 
        /// the data providers in your application</param>
        IDataCatalog Register(Assembly assembly, Func<Type, object> factoryFunc);

        /// <summary>
        /// Checks to see if the data context contains data of the specified
        /// type and if not adds it by locating a suitable data provider
        /// </summary>
        /// <typeparam name="T">The type of data that we need to be in context</typeparam>
        /// <param name="renderContext">The response rendering context</param>
        /// <param name="dataContext">The data context to search and add to if necessary</param>
        /// <returns>Returns data from the data context</returns>
        T Ensure<T>(IRenderContext renderContext, IDataContext dataContext) where T : class;
    }
}
