using System;

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
        /// Takes all of the data needs of this element and resolves
        /// them with the scope provider. You only need to do this once.
        /// </summary>
        /// <param name="scopeProvider">The scope provider associated
        /// with this element</param>
        void ResolveDependencies(IDataScopeProvider scopeProvider);

        /// <summary>
        /// Call this method to indicate that the specified data is required
        /// for this component to function and an absence of this data is a 
        /// fatal error
        /// </summary>
        /// <typeparam name="T">The type of data that is required</typeparam>
        /// <param name="scopeName">Optional scope name used to bind to data from
        /// an ancestor where there are multiple data in context of the same type</param>
        void NeedsData<T>(string scopeName = null);

        /// <summary>
        /// Call this method to indicate that the specified data is required
        /// for this component to function and an absence of this data is a 
        /// fatal error
        /// </summary>
        /// <param name="dataType">The type of data that is required</param>
        /// <param name="scopeName">Optional scope name used to bind to data from
        /// an ancestor where there are multiple data in context of the same type</param>
        void NeedsData(Type dataType, string scopeName = null);

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
        /// need the same data provider it will only execute once.
        /// </summary>
        /// <param name="dataProvider">The data provider that this element needs</param>
        /// <param name="dependency">Optional dependency information to pass to the
        /// data provider in case this provider can provide multiple kinds of data</param>
        void NeedsProvider(IDataProvider dataProvider, IDataDependency dependency = null);
    }
}
