using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// This interface is implemented by components that can
    /// request specific data to be available at runtime. The
    /// DataConsumer class can be used to implement the Mixin pattern
    /// for this.
    /// </summary>
    public interface IDataConsumer
    {
        /// <summary>
        /// Call this method to indicate that the specified data is required
        /// for this component to function and an absence of this data is a 
        /// fatal error
        /// </summary>
        /// <typeparam name="T">The type of data that is required</typeparam>
        /// <param name="scopeName">Optional scope name used to bind to data from
        /// an ancestor where there are multiple data in context of the same type</param>
        void HasDependency<T>(string scopeName = null);

        /// <summary>
        /// Call this method to indicate that the specified data is required
        /// for this component to function and an absence of this data is a 
        /// fatal error
        /// </summary>
        /// <param name="dataType">The type of data that is required</param>
        /// <param name="scopeName">Optional scope name used to bind to data from
        /// an ancestor where there are multiple data in context of the same type</param>
        void HasDependency(Type dataType, string scopeName = null);

        /// <summary>
        /// Call this method to indicate that the specified data will be used
        /// by this component when available, but the component can also function
        /// without it
        /// </summary>
        /// <typeparam name="T">The type of data that can be used</typeparam>
        /// <param name="scopeName">Optional scope name used to bind to data from
        /// an ancestor where there are multiple data in context of the same type</param>
        void CanUseData<T>(string scopeName = null);

        /// <summary>
        /// Call this method to indicate that the specified data will be used
        /// by this component when available, but the component can also function
        /// without it
        /// </summary>
        /// <param name="dataType">The type of data that can be used</param>
        /// <param name="scopeName">Optional scope name used to bind to data from
        /// an ancestor where there are multiple data in context of the same type</param>
        void CanUseData(Type dataType, string scopeName = null);

        /// <summary>
        /// Call this method to specify a data provider that needs to run
        /// to provide the data that this control needs. If multiple elements
        /// </summary>
        void HasDependency(IDataProvider dataProvider, IDataDependency dependency = null);

        /// <summary>
        /// Call this method to specify a data supply that needs to run
        /// to provide the data that this control needs.
        /// </summary>
        void HasDependency(IDataSupply dataSupply);

        /// <summary>
        /// Returns information about what this data consumer needs. This information
        /// is used by the data context builder to resolve dependencies and build the
        /// data context
        /// </summary>
        IDataConsumerNeeds Needs { get; }
    }
}
