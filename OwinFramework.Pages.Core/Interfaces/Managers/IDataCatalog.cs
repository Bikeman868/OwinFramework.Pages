using System;
using System.Collections.Generic;
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
        /// Adds data to the data context by locating and executing data providers
        /// </summary>
        /// <param name="types">The types of data missing from the data context</param>
        /// <param name="scopeOrder">The scopes in the data context heirachy in the order 
        /// of preference. The data catalog will satisfy the data need using the 
        /// first matching scope from this list</param>
        /// <param name="renderContext">The response rendering context</param>
        /// <param name="dataContext">The data context to add the missing data to</param>
        void AddData(
            IList<Type> types, 
            IList<string> scopeOrder,
            IRenderContext renderContext, 
            IDataContext dataContext);
    }
}
