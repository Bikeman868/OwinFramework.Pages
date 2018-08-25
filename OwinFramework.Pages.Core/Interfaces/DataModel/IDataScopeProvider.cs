using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// This interface is implemented by elements that can establish a new
    /// data scope during the rendering operation.
    /// </summary>
    public interface IDataScopeProvider
    {

/*******************************************************************
* These interface members can be used to set up the scope provider
* prior to initialization.
*******************************************************************/

        /// <summary>
        /// A unique ID for this scope provider. This is used to find the
        /// corresponding data context during rendering operations.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The name of the element that this is providing scope to
        /// </summary>
        string ElementName { get; set; }

        /// <summary>
        /// Retrieves debugging information from this scope provider
        /// </summary>
        /// <param name="parentDepth">How deeply to traverse the parents. Pass
        /// -1 to go all the way to the top of the tree, 0 for no parent information,
        /// 1 for the immediate parent, 2 for grandparents etc</param>
        /// <param name="childDepth">How deeply to recurse the chilren. Pass -1 to recurse
        /// indefinately to the buttom of the tree. Pass 0 for no children, 1 for
        /// children but no grandchildren etc.</param>
        DebugDataScopeProvider GetDebugInfo(int parentDepth, int childDepth);

        /// <summary>
        /// Adds a scope to this scope provider. Any requests for data
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
        /// each request to supply data required by the element. Can be called
        /// before or after initialization
        /// </summary>
        /// <param name="supplier">A supplier of data to add to this data scope</param>
        /// <param name="dependencyToSupply">The data that we want this supplier to supply</param>
        IDataSupply AddSupplier(
            IDataSupplier supplier, 
            IDataDependency dependencyToSupply);

        /// <summary>
        /// Adds a data supply to the list of what must be supplied to the 
        /// data context for rendering. Can be called before or after
        /// initialization.
        /// </summary>
        /// <remarks>Note that adding a data supply directly like this
        /// give the scope provider no clue about what data is being
        /// supplied, and therefore it does not know which dependencies
        /// if any will be satified. Whenever possible add a DataSupplier
        /// instead because it can let the data scope know which 
        /// dependencies it satisfies.</remarks>
        void AddSupply(IDataSupply supply);

        /// <summary>
        /// Creates a new data scope provider that has the same scopes
        /// and providers as this one. 
        /// Call this method before initialization.
        /// </summary>
        IDataScopeProvider CreateInstance();

/*******************************************************************
* These interface members are used to build scope providers into a
* tree heirachy. This has to happen before dependencies can be 
* resolved.
*******************************************************************/

        /// <summary>
        /// Constructs the parent/child tree of data scope providers and
        /// gets the scope provider into a state where it can resolve
        /// data needs
        /// </summary>
        void Initialize(IDataScopeProvider parent);

        /// <summary>
        /// Returns the parent scope
        /// </summary>
        IDataScopeProvider Parent { get; }

        /// <summary>
        /// Adds a child data scope provider establishing a nested scope
        /// for data dependency resolution
        /// </summary>
        void AddChild(IDataScopeProvider child);

/*******************************************************************
* These interface members can be used after initialization to 
* tell the scope provider what data it needs to be able to supply.
* These data needs might be met by this scope provider or deferred
* to the parent.
*******************************************************************/

        /// <summary>
        /// Adds a dependency on data. A suitable provider will be located
        /// and added to this scope or a parent scope according to where this
        /// scope is handled.
        /// This method can only be called after initialization
        /// </summary>
        IDataSupply AddDependency(IDataDependency dependency);

        /// <summary>
        /// Must be called after initialization, adds data suppliers to
        /// this scope or its ancestors to satisfy the needs of this
        /// data consumer
        /// </summary>
        IList<IDataSupply> AddConsumer(IDataConsumer consumer);

    /*******************************************************************
    * These interface members build the data context for a request using
    * the data supplies that were resolved from the data needs
    *******************************************************************/

        /// <summary>
        /// This should only be called on the root, it recursively traverses the
        /// tree of descendants creating a tree of data contexts in the render 
        /// context.
        /// </summary>
        /// <param name="renderContext">A newly instantiated render context to
        /// set the data context for</param>
        void SetupDataContext(IRenderContext renderContext);

        /// <summary>
        /// This is called recursively to construct a tree of data contexts
        /// You should not call this method from your application
        /// </summary>
        void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext);

        /// <summary>
        /// Sets the current Data property of the render context to the 
        /// data for this scope provider
        /// </summary>
        /// <returns>The previously selected data context so that you can
        /// restore this later</returns>
        IDataContext SetDataContext(IRenderContext renderContext);

    /*******************************************************************
    * These interface members are used to dynamically add new dependencies
    * discovered at runtime when the application tries to retrieve data
    * that it did not declare as a data need.
    *******************************************************************/

        /// <summary>
        /// Used to determine if the particular type is available from this
        /// scope provider.
        /// </summary>
        /// <param name="dependency">The type of data we are looking for</param>
        bool IsInScope(IDataDependency dependency);

        /// <summary>
        /// This is called in the case where dependencies are missing from the 
        /// data context because the dependencies were not correctly specified
        /// up front. It adds the new dependency going forward
        /// </summary>
        /// <remarks>This is an expensive method that tears down and
        /// recreates the whole data context, running all of the data supplies
        /// again. This only happens when the application tries to use data
        /// that it did not deplare that it needed. The missing dependecny is
        /// added to the scope provider so that this will not happen again
        /// on subsequent requests for the same page or service.</remarks>
        void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency);
    }
}
