using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A data provider reads data from somewhere else, could be a service,
    /// a database, in memory, or provided by another data provider, and
    /// adds this data to the data context of the request.
    /// Components that bind to data will use the data that is in this
    /// data context. When multiple components on the page bind to the
    /// same data the data provider will only be executed once.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Gets and sets the name of this data provider. This allows other elements to
        /// reference this provider by name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets and sets the package. Setting a package puts the name of the data provider
        /// into a namespace
        /// </summary>
        IPackage Package { get; set; }

        /// <summary>
        /// The runtime will call this for each page request that needs the
        /// type of data provided by this instance.
        /// </summary>
        /// <param name="renderContext">The request that is being handled</param>
        /// <param name="dataContext">The data context to use to get dependant data and where
        /// new data should be added</param>
        /// <param name="dependency">The data dependency to satisfy or null if 
        /// this provider was not executed as a resault of a specific dependency</param>
        void Satisfy(IRenderContext renderContext, IDataContext dataContext, IDataDependency dependency);
    }
}
