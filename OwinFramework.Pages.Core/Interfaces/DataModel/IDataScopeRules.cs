using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// This interface is implemented by elements that can establish a new
    /// data scope during the rendering operation. Within the framework the
    /// Page, Service and zone do this but any application defined element 
    /// can do this too.
    /// </summary>
    public interface IDataScopeRules
    {
        /// <summary>
        /// The name of the element that this is providing scope rules for
        /// </summary>
        string ElementName { get; set; }

        /// <summary>
        /// Adds a scope to this scope rules. Any requests for data
        /// that match this scope will stop here. Any requests for data
        /// outside of the scopes are propogated to the parent
        /// </summary>
        /// <param name="type">The type of data handle or null to
        /// handle alll types of data</param>
        /// <param name="scopeName">The scope name to handle or null 
        /// to handle all scopes</param>
        void AddScope(Type type, string scopeName);

        /// <summary>
        /// Adds a supplier to the list of suppliers that must be run for
        /// each request to supply data required by the element
        /// </summary>
        /// <param name="supplier">A supplier of data to add to this data scope</param>
        /// <param name="dependencyToSupply">The data that we want this supplier to supply</param>
        void AddSupplier(
            IDataSupplier supplier, 
            IDataDependency dependencyToSupply);

        /// <summary>
        /// Adds a data supply to the list of what must be supplied to the 
        /// data context for rendering
        /// </summary>
        /// <remarks>Note that adding a data supply directly like this
        /// gives the scope rules no clue about what data is being
        /// supplied, and therefore it does not know which dependencies
        /// if any will be satified. Whenever possible add a DataSupplier
        /// instead because it can let the data scope know which 
        /// dependencies it satisfies.</remarks>
        void AddSupply(IDataSupply supply);

        /// <summary>
        /// Returns the scopes that should be resolved by this element
        /// </summary>
        IList<IDataScope> DataScopes { get; }

        /// <summary>
        /// Returns the data suppliers that must be executed to supply data to this
        /// element
        /// </summary>
        IList<Tuple<IDataSupplier, IDataDependency>> SuppliedDependencies { get; }

        /// <summary>
        /// Returns the data that should be explicitly injected into this data scope
        /// </summary>
        IList<IDataSupply> DataSupplies { get; }
    }
}
