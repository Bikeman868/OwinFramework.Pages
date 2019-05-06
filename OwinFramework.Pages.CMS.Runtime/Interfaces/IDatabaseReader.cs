using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    public interface IDatabaseReader
    {
        #region Website versions

        /// <summary>
        /// Retrieves a list of website versions
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which versions to return. 
        /// If null is passed then all versions are returned</param>
        T[] GetWebsiteVersions<T>(Func<WebsiteVersionRecord, T> map, Func<WebsiteVersionRecord, bool> predicate = null);

        #endregion

        #region Website version elements

        /// <summary>
        /// Retrieves a list of page versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of this website version to retrieve</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsitePages<T>(long websiteVersionId, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of page versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionName">The name of the website version to retrieve</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsitePages<T>(string websiteVersionName, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of layout versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of this website version to retrieve</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteLayouts<T>(long websiteVersionId, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of layout versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionName">The name of the website version to retrieve</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteLayouts<T>(string websiteVersionName, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of region versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of this website version to retrieve</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteRegions<T>(long websiteVersionId, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of region versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionName">The name of the website version to retrieve</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteRegions<T>(string websiteVersionName, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of bindable data types for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of this website version to retrieve</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteDataTypes<T>(long websiteVersionId, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of bindable data types for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionName">The name of the website version to retrieve</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which records to return. 
        /// If null is passed then all records are returned</param>
        T[] GetWebsiteDataTypes<T>(string websiteVersionName, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate = null);

        #endregion

        #region Elements

        /// <summary>
        /// Retrieves the properties to set on a specific element version
        /// </summary>
        /// <param name="elementVersionId">The element version to get properties for</param>
        /// <returns></returns>
        IDictionary<string, string> GetElementProperties(long elementVersionId);

        /// <summary>
        /// Retrieves a list of the versions that exist for an element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elementId"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        T[] GetElementVersions<T>(long elementId, Func<ElementVersionRecordBase, T> map);
       
        /// <summary>
        /// Retrieves a single page version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="pageId">The unique ID of the page to return</param>
        /// <param name="version">The version of the page to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetPage<T>(long pageId, int version, Func<PageRecord, PageVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single layout version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="layoutId">The unique ID of the layout to return</param>
        /// <param name="version">The version of the layout to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetLayout<T>(long layoutId, int version, Func<LayoutRecord, LayoutVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single layout version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="regionId">The unique ID of the region to return</param>
        /// <param name="version">The version of the region to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetRegion<T>(long regionId, int version, Func<RegionRecord, RegionVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single page version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="pageVersionId">The unique ID of the page version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetPage<T>(long pageVersionId, Func<PageRecord, PageVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single layout version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="layoutVersionId">The unique ID of the layout version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetLayout<T>(long layoutVersionId, Func<LayoutRecord, LayoutVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single layout version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="regionVersionId">The unique ID of the region version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetRegion<T>(long regionVersionId, Func<RegionRecord, RegionVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single layout version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="dataTypeVersionId">The unique ID of the data type version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetDataType<T>(long dataTypeVersionId, Func<DataTypeRecord, DataTypeVersionRecord, T> map);

        #endregion
    }
}
