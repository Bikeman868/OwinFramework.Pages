using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    public interface IDatabaseReader
    {
        #region Environments

        /// <summary>
        /// Retrieves a list of environments
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which environments to return. 
        /// If null is passed then all environments are returned</param>
        T[] GetEnvironments<T>(Func<EnvironmentRecord, T> map, Func<EnvironmentRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a single environment record by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="environmentId">The unique ID of the environment to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetEnvironment<T>(long environmentId, Func<EnvironmentRecord, T> map);

        #endregion

        #region Website versions

        /// <summary>
        /// Retrieves a list of website versions
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which versions to return. 
        /// If null is passed then all versions are returned</param>
        T[] GetWebsiteVersions<T>(Func<WebsiteVersionRecord, T> map, Func<WebsiteVersionRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a single website version record by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of the website version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetWebsiteVersion<T>(long websiteVersionId, Func<WebsiteVersionRecord, T> map);

        #endregion

        #region Website version elements

        /// <summary>
        /// Retrieves a list of page versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of this website version to retrieve</param>
        /// <param name="scenarioName">For A/B testing this is the name of the test scenario</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsitePages<T>(long websiteVersionId, string scenarioName, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of page versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionName">The name of the website version to retrieve</param>
        /// <param name="scenarioName">For A/B testing this is the name of the test scenario</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsitePages<T>(string websiteVersionName, string scenarioName, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of layout versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of this website version to retrieve</param>
        /// <param name="scenarioName">For A/B testing this is the name of the test scenario</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteLayouts<T>(long websiteVersionId, string scenarioName, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of layout versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionName">The name of the website version to retrieve</param>
        /// <param name="scenarioName">For A/B testing this is the name of the test scenario</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteLayouts<T>(string websiteVersionName, string scenarioName, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of region versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of this website version to retrieve</param>
        /// <param name="scenarioName">For A/B testing this is the name of the test scenario</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteRegions<T>(long websiteVersionId, string scenarioName, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of region versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionName">The name of the website version to retrieve</param>
        /// <param name="scenarioName">For A/B testing this is the name of the test scenario</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteRegions<T>(string websiteVersionName, string scenarioName, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of bindable data types for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of this website version to retrieve</param>
        /// <param name="scenarioName">For A/B testing this is the name of the test scenario</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteDataTypes<T>(long websiteVersionId, string scenarioName, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of bindable data types for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionName">The name of the website version to retrieve</param>
        /// <param name="scenarioName">For A/B testing this is the name of the test scenario</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteDataTypes<T>(string websiteVersionName, string scenarioName, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of bindable data types for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of this website version to retrieve</param>
        /// <param name="scenarioName">For A/B testing this is the name of the test scenario</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteComponents<T>(long websiteVersionId, string scenarioName, Func<WebsiteVersionComponentRecord, T> map, Func<WebsiteVersionComponentRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of bindable data types for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionName">The name of the website version to retrieve</param>
        /// <param name="scenarioName">For A/B testing this is the name of the test scenario</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteComponents<T>(string websiteVersionName, string userSegment, Func<WebsiteVersionComponentRecord, T> map, Func<WebsiteVersionComponentRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of the website version/scenario combinations where a given element version is used
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="elementVersionId">The element version to get usages for</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetElementUsage<T>(long elementVersionId, Func<WebsiteVersionRecordBase, T> map, Func<WebsiteVersionRecordBase, bool> predicate = null);

        #endregion

        #region All elements of a type

        T[] GetPages<T>(Func<PageRecord, T> map, Func<PageRecord, bool> predicate = null);
        T[] GetLayouts<T>(Func<LayoutRecord, T> map, Func<LayoutRecord, bool> predicate = null);
        T[] GetRegions<T>(Func<RegionRecord, T> map, Func<RegionRecord, bool> predicate = null);
        T[] GetDataTypes<T>(Func<DataTypeRecord, T> map, Func<DataTypeRecord, bool> predicate = null);
        T[] GetDataScopes<T>(Func<DataScopeRecord, T> map, Func<DataScopeRecord, bool> predicate = null);
        T[] GetComponents<T>(Func<ComponentRecord, T> map, Func<ComponentRecord, bool> predicate = null);
        T[] GetModules<T>(Func<ModuleRecord, T> map, Func<ModuleRecord, bool> predicate = null);

        #endregion

        #region Elements

        /// <summary>
        /// Retrieves the property values to set on a specific element version
        /// </summary>
        /// <param name="elementVersionId">The element version to get properties for</param>
        /// <returns></returns>
        IDictionary<string, object> GetElementPropertyValues(long elementVersionId);

        /// <summary>
        /// Retrieves a list of the versions that exist for an element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elementId"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        T[] GetElementVersions<T>(long elementId, Func<ElementVersionRecordBase, T> map);
       

        /// <summary>
        /// Retrieves a single page by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="pageId">The unique ID of the page to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetPage<T>(long pageId, Func<PageRecord, T> map);

        /// <summary>
        /// Retrieves a single layout by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="layoutId">The unique ID of the layout to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetLayout<T>(long layoutId, Func<LayoutRecord, T> map);

        /// <summary>
        /// Retrieves a single region by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="regionId">The unique ID of the region to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetRegion<T>(long regionId, Func<RegionRecord, T> map);

        /// <summary>
        /// Retrieves a single data scope by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="dataScopeId">The unique ID of the data scope to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetDataScope<T>(long dataScopeId, Func<DataScopeRecord, T> map);

        /// <summary>
        /// Retrieves a single data type by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="regionId">The unique ID of the data type to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetDataType<T>(long dataTypeId, Func<DataTypeRecord, T> map);


        /// <summary>
        /// Retrieves a single page version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="pageId">The unique ID of the page to return</param>
        /// <param name="version">The version of the page to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetPageVersion<T>(long pageId, int version, Func<PageRecord, PageVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single layout version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="layoutId">The unique ID of the layout to return</param>
        /// <param name="version">The version of the layout to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetLayoutVersion<T>(long layoutId, int version, Func<LayoutRecord, LayoutVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single region version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="regionId">The unique ID of the region to return</param>
        /// <param name="version">The version of the region to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetRegionVersion<T>(long regionId, int version, Func<RegionRecord, RegionVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single data type version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="dataTypeId">The unique ID of the data type to return</param>
        /// <param name="version">The version of the data type to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetDataTypeVersion<T>(long dataTypeId, int version, Func<DataTypeRecord, DataTypeVersionRecord, T> map);


        /// <summary>
        /// Retrieves a single page version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="pageVersionId">The unique ID of the page version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetPageVersion<T>(long pageVersionId, Func<PageRecord, PageVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single layout version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="layoutVersionId">The unique ID of the layout version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetLayoutVersion<T>(long layoutVersionId, Func<LayoutRecord, LayoutVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single region version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="regionVersionId">The unique ID of the region version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetRegionVersion<T>(long regionVersionId, Func<RegionRecord, RegionVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single data type version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="dataTypeVersionId">The unique ID of the data type version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetDataTypeVersion<T>(long dataTypeVersionId, Func<DataTypeRecord, DataTypeVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single component version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="componentVersionId">The unique ID of the component version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetComponentVersion<T>(long componentVersionId, Func<ComponentRecord, ComponentVersionRecord, T> map);

        #endregion

        #region Change history

        HistoryPeriodRecord GetHistory(string recordType, long id, string bookmark);
        HistoryEventRecord[] GetHistorySummary(long summaryId);

        #endregion
    }
}
