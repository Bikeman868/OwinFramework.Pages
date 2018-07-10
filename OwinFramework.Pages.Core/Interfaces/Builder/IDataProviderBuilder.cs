using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the data provider builder to construct data providers using a fluent syntax
    /// </summary>
    public interface IDataProviderBuilder
    {
        /// <summary>
        /// Starts building a new data provider or configuring an existing data provider
        /// </summary>
        /// <param name="dataProviderInstance">Pass an instance that derives from DataProvider
        /// to configure it directly, or pass any other instance type or null to 
        /// construct an instance of the DataProvider class</param>
        /// <param name="declaringType">Type type to extract custom attributes from
        /// that can also define the behaviour of the data provider</param>
        /// <param name="package">Optional package adds a namespace to this component</param>
        IDataProviderDefinition BuildUpDataProvider(object dataProviderInstance = null, Type declaringType = null, IPackage package = null);
    }
}
