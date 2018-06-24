using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Parent  or null if this is the root
        /// </summary>
        IDataScopeProvider Parent { get; }

        /// <summary>
        /// Children
        /// </summary>
        IList<IDataScopeProvider> Children { get; }

        /// <summary>
        /// This provider introduces a new scope foe these kinds of data. These
        /// definitions are used once only to create the data context definitions
        /// by resolving types and scope names in the data provider catalog
        /// </summary>
        IList<IDataScope> DataScopes { get; }

        /// <summary>
        /// This list is produced initially by resolving the DataScopes in the
        /// data provider catalog. There is then a process of pruning anything
        /// in the descendents that duplicates an ancestor in the tree.
        /// At runtime when components request data that was not declared ahead
        /// of time these need are added here so that the framework learns and
        /// becomes more efficient on subsequent requests.
        /// </summary>
        IDataContextDefinition DataContextDefinition { get; }

        /// <summary>
        /// Used to determine if the particular type is available from this
        /// scope provider. When it is not available the parent scope provider
        /// will be examined etc.
        /// </summary>
        /// <param name="type">The type of data we are looking for</param>
        /// <param name="scopeName">The name of the scope for this data</param>
        bool Provides(Type type, string scopeName);

        /// <summary>
        /// This should be called once only on the root to traverse the tree
        /// of data scope providers and populate their DataContextDefinitions
        /// by resolving DataScopes in the data provider catalog.
        /// </summary>
        void ResolveDataScopes();

        /// <summary>
        /// This should only be called on the root, it recursively traverses the
        /// tree of descendants creating a tree of data contexts in the render 
        /// context.
        /// </summary>
        /// <param name="renderContext">A newly instantiated rander context to
        /// set the data context for</param>
        void SetupDataContext(IRenderContext renderContext);

        /// <summary>
        /// This is called in the case where dependencies are missing from the 
        /// data context because the dependencies were not correctly specified
        /// up front. It adds the new dependency going forward
        /// </summary>
        void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency);
    }
}
