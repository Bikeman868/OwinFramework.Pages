using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// These are associated with data scope providers (pages and regions).
    /// They define the data providers that should be used for each type of 
    /// data within this scope.
    /// </summary>
    public interface IDataContextDefinition
    {
        /// <summary>
        /// The list of data providers that must be executed to
        /// establish a data context
        /// </summary>
        IList<IDataProviderDefinition> DataProviders { get; }

        /// <summary>
        /// Adds a new data provider definition
        /// </summary>
        void Add(IDataProviderDefinition dataProvider);

        /// <summary>
        /// Adds a new data provider definition
        /// </summary>
        void Add(IDataProvider dataProvider, IDataDependency dependancy = null);
    }
}
