﻿using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// This interface is implemented by objects that can resolve
    /// a data dependencies and add dependent data to the data context
    /// during a rendering operation.
    /// </summary>
    public interface IDataContextBuilder
    {
        /// <summary>
        /// A unique ID for this context builder. This is used to find the
        /// corresponding data context during rendering operations.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Creates a new data context builder as a child of the current one.
        /// If the child can not resolve data dependencies then it will fall back
        /// to its parent
        /// </summary>
        /// <param name="dataScopeRules">The dependencies to resolve in 
        /// the child scope</param>
        IDataContextBuilder AddChild(IDataScopeRules dataScopeRules);

        /// <summary>
        /// Adds a dependency on data. A suitable provider will be located
        /// and added to this data context builder
        /// </summary>
        IDataSupply AddDependency(IDataDependency dependency);

        /// <summary>
        /// Adds data suppliers to this data context builder
        /// or its ancestors to satisfy the needs of this
        /// data consumer
        /// </summary>
        IList<IDataSupply> AddConsumer(IDataConsumer consumer);

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
        /// You should NOT call this method from your application
        /// </summary>
        void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext);
        
        /// <summary>
        /// Used to determine if the particular type is available from this
        /// data context builder.
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
        /// that it did not declare that it needed. The missing dependecny is
        /// added to the data context builder so that this will not happen again
        /// on subsequent requests for the same runnable.</remarks>
        void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency);
    }
}
