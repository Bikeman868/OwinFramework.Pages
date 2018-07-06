using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// This interface is implemented by components that can
    /// request specific data to be available at runtime. The
    /// DataConsumer class can be used to implement the MixIn pattern
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
        /// Examines the data providers in scope to see if all of its dependencies have
        /// been met and adds any missing data supplies so that the scope will satisfy
        /// all of its dependencies
        /// </summary>
        /// <param name="dataScope">The data scope to add missing dependencies to</param>
        /// <returns>A list of all of the data providers that this element depends
        /// on. The scope provider can use this list to ensure that providers
        /// are executed in the correct order to satisfy all dependencies</returns>
        IList<IDataSupply> GetDependencies(IDataScopeProvider dataScope);
    }
}
