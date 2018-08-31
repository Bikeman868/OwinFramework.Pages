using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Defines a mixin that indicates that this object can supply data. Supplying
    /// data in this context means at runtime it can retrieve data from somewhere
    /// and add it to the context of a page rendering operation.
    /// Objects can be both suppliers and consumers of data at the same time
    /// </summary>
    public interface IDataSupplier
    {
        /// <summary>
        /// Returns a list of the data types that this supplier can supply.
        /// Call the IsSupplierOf() method to discover if it can provide the type
        /// in a specific scope
        /// </summary>
        IList<Type> SuppliedTypes { get; }

        /// <summary>
        /// This dependency will be used in cases where the application declares
        /// a dependency on a supplier without specifying the particular type and
        /// scope
        /// </summary>
        IDataDependency DefaultDependency { get; }

        /// <summary>
        /// Indicates whether this supplier supplies data for a specific scope
        /// </summary>
        bool IsScoped { get; }

        /// <summary>
        /// Adds a lambda expresson that will add a specific type of data to 
        /// the data context.
        /// </summary>
        void Add(IDataDependency dependency, Action<IRenderContext, IDataContext, IDataDependency> action);

        /// <summary>
        /// Tests whether this supplier can supply this type of data
        /// </summary>
        bool IsSupplierOf(IDataDependency dependency);

        /// <summary>
        /// Gets an instance that will add a specific type of data to the render context
        /// when executed
        /// </summary>
        /// <param name="dependency">The type of data to supply</param>
        IDataSupply GetSupply(IDataDependency dependency);
    }
}
