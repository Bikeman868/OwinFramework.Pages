using System;
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
        /// Checks to see if the data context contains data of the specified
        /// type and if not adds it by locating a suitable data provider
        /// </summary>
        /// <typeparam name="T">The type of data that we need to be in context</typeparam>
        /// <param name="dataContext">The data context to search and add to if necessary</param>
        /// <returns>Returns data from the data context</returns>
        T Ensure<T>(IDataContext dataContext) where T : class;
    }
}
