using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Defines a mixin that indicates that this object can supply data.
    /// Objects can be both suppliers and consumers of data at the same time
    /// </summary>
    public interface IDataSupplier
    {
        /// <summary>
        /// Returns a list of the data types that this supplier can supply.
        /// Call the CanSupply() method to discover if it can provide the type
        /// in a specific scope
        /// </summary>
        IList<Type> SuppliedTypes { get; }

        /// <summary>
        /// Indicates whether this supplier supplies data for a specific scope
        /// </summary>
        bool IsScoped { get; }

        /// <summary>
        /// Adds a type of data that this supplier can supply
        /// </summary>
        void Add(IDataDependency dependency, Action<IRenderContext, IDataContext, IDataDependency> action);

        /// <summary>
        /// Tests whether this supplier can supply this type of data
        /// </summary>
        bool CanSupply(IDataDependency dependency);

        /// <summary>
        /// Gets an instance that will add a specific type of data to the render context
        /// when executed
        /// </summary>
        /// <param name="dependency">The type of data to supply</param>
        /// <param name="dependencies">Other supplies that this one depends on</param>
        IDataSupply GetSupply(IDataDependency dependency, IList<IDataSupply> dependencies = null);
    }
}
