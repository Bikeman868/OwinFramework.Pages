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
        /// <summary>
        /// A unique ID for this scope provider. This is used to find the
        /// corresponding data context during rendering operations.
        /// </summary>
        int Id { get; }

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
        /// Returns the parent scope
        /// </summary>
        IDataScopeProvider Parent { get; }

        /// <summary>
        /// Adds a child data scope provider establishing a nested scope
        /// for data dependency resolution
        /// </summary>
        void AddChild(IDataScopeProvider child);

        /// <summary>
        /// Specifies the parent data scope provider. This is established 
        /// when walking the element tree during initialization
        /// </summary>
        void SetParent(IDataScopeProvider parent);

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
        /// Adds a data provider that should be executed when the data context
        /// is created
        /// </summary>
        /// <param name="dataProvider">The data provider to execute</param>
        /// <param name="dependency">The dependency that the provider will 
        /// be asked to satisfy</param>
        void Add(IDataProvider dataProvider, IDataDependency dependency = null);

        /// <summary>
        /// Adds a dependency on data. A suitable provider will be located
        /// and added to this scope or a parent scope according to where this
        /// scope is handled
        /// </summary>
        /// <param name="dependency">The data that the element depends on</param>
        void Add(IDataDependency dependency);

        /// <summary>
        /// Used to determine if the particular type is available from this
        /// scope provider.
        /// </summary>
        /// <param name="type">The type of data we are looking for</param>
        /// <param name="scopeName">The name of the scope for this data</param>
        bool IsInScope(Type type, string scopeName);

        /// <summary>
        /// This should be called once only on the root to traverse the tree
        /// of data scope providers and populate their data providers
        /// by resolving dependencies in the data provider catalog.
        /// </summary>
        /// <param name="existingProviders">A list of data providers already
        /// added to the context by ancestors that should not be added
        /// again here</param>
        void ResolveDataProviders(IList<IDataProvider> existingProviders);

        /// <summary>
        /// Returns a list of the data providers that will execute when
        /// a data context is established for this scope
        /// </summary>
        List<IDataProvider> DataProviders { get; }

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
        void BuildDataContextTree(IRenderContext renderContext, IDataContext dataContext, bool isParentDataContext);
    
        /// <summary>
        /// This is called in the case where dependencies are missing from the 
        /// data context because the dependencies were not correctly specified
        /// up front. It adds the new dependency going forward
        /// </summary>
        void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency);
    }
}
