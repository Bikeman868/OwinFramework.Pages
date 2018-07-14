using System;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the fluent syntax for building data providers.
    /// </summary>
    public interface IDataProviderDefinition
    {
        /// <summary>
        /// Sets the name of the component so that it can be referenced
        /// by other elements
        /// </summary>
        IDataProviderDefinition Name(string name);

        /// <summary>
        /// Specifies that this data provider is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="package">The package that this data provider is
        /// part of</param>
        /// <returns></returns>
        IDataProviderDefinition PartOf(IPackage package);

        /// <summary>
        /// Specifies that this data provider is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="packageName">The name of the package that this 
        /// data provider is part of</param>
        /// <returns></returns>
        IDataProviderDefinition PartOf(string packageName);

        /// <summary>
        /// Specifies that this data provider is dendenant on another data
        /// provider that must be run in the data context prior to this one
        /// </summary>
        /// <param name="dataProviderName">The name of the dependent data provider</param>
        IDataProviderDefinition DependsOn(string dataProviderName);

        /// <summary>
        /// Specifies that this data provider is dendenant on another data
        /// provider that must be run in the data context prior to this one
        /// </summary>
        /// <param name="dataProvider">The dependent data provider</param>
        IDataProviderDefinition DependsOn(IDataProvider dataProvider);

        /// <summary>
        /// Adds metadata to the data provider that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <typeparam name="T">The type of data that this data provider binds to</typeparam>
        /// <param name="scope">Optional scope name used to resolve which data provider
        /// will source the data</param>
        IDataProviderDefinition BindTo<T>(string scope = null) where T : class;

        /// <summary>
        /// Adds metadata to the data provider that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <param name="dataType">The type of data that this data provider wil request</param>
        /// <param name="scope">Optional scope name used to resolve which data provider
        /// will source the data</param>
        IDataProviderDefinition BindTo(Type dataType, string scope = null);

        /// <summary>
        /// Configures a type of data that this data provider can provide
        /// </summary>
        /// <typeparam name="T">The type of data that this data provider provides</typeparam>
        /// <param name="scope">Optional scope name used to resolve requests for data</param>
        IDataProviderDefinition Provides<T>(string scope = null) where T : class;

        /// <summary>
        /// Configures a type of data that this data provider can provide
        /// </summary>
        /// <param name="dataType">The type of data that this data provider wil provide</param>
        /// <param name="scope">Optional scope name used to resolve requests for data</param>
        IDataProviderDefinition Provides(Type dataType, string scope = null);

        /// <summary>
        /// Configures a type of data that this data provider can provide
        /// </summary>
        /// <param name="dataType">The type of data that this data provider wil provide</param>
        /// <param name="action">The method that will be used to get the data when required for rendering</param>
        /// <param name="scope">Optional scope name used to resolve requests for data</param>
        IDataProviderDefinition Provides(
            Type dataType, 
            Action<IRenderContext, IDataContext, IDataDependency> action, 
            string scope = null);

        /// <summary>
        /// Configures a type of data that this data provider can provide
        /// </summary>
        /// <typeparam name="T">The type of data that this data provider provides</typeparam>
        /// <param name="action">The method that will be used to get the data when required for rendering</param>
        /// <param name="scope">Optional scope name used to resolve requests for data</param>
        IDataProviderDefinition Provides<T>(
            Action<IRenderContext, IDataContext, IDataDependency> action,
            string scope = null);

        /// <summary>
        /// Builds the data provider
        /// </summary>
        IDataProvider Build();
    }
}
