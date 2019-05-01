using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    public interface IDatabaseReader
    {
        /// <summary>
        /// Retrieves a list of website versions
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which versions to return. 
        /// If null is passed then all versions are returned</param>
        IList<T> GetWebsiteVersions<T>(Func<WebsiteVersionRecord, T> map, Func<WebsiteVersionRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of page versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionId">The unique ID of this website version to retrieve</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which versions to return. 
        /// If null is passed then all versions are returned</param>
        IList<T> GetWebsiteVersionPages<T>(long websiteVersionId, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of page versions for a website version
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="websiteVersionName">The name of the website version to retrieve</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which versions to return. 
        /// If null is passed then all versions are returned</param>
        IList<T> GetWebsiteVersionPages<T>(string websiteVersionName, Func<WebsiteVersionPageRecord, T> map, Func<WebsiteVersionPageRecord, bool> predicate = null);

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
        IList<T> GetElementVersions<T>(long elementId, Func<ElementVersionRecordBase, T> map);

        /// <summary>
        /// Retrieves a single page version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="elementId">The unique ID of the page to return</param>
        /// <param name="version">The version of the page to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetPage<T>(long elementId, int version, Func<PageRecord, PageVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single layout version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="elementId">The unique ID of the layout to return</param>
        /// <param name="version">The version of the layout to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetLayout<T>(long elementId, int version, Func<LayoutRecord, LayoutVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single page version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="elementVersionId">The unique ID of the page version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetPage<T>(long elementVersionId, Func<PageRecord, PageVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single layout version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="elementVersionId">The unique ID of the layout version to return</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetLayout<T>(long elementVersionId, Func<LayoutRecord, LayoutVersionRecord, T> map);
    }
}
