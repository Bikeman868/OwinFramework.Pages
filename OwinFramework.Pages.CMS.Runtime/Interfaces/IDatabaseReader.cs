using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        IList<T> GetVersions<T>(Func<VersionRecord, T> map, Func<VersionRecord, bool> predicate = null);

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
        IList<T> GetElementVersions<T>(long elementId, Func<VersionRecord, ElementVersionRecordBase, T> map);

        /// <summary>
        /// Retrieves a single page version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="elementId">The unique ID of the page to return</param>
        /// <param name="versionName">The name of the website version to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetPage<T>(long elementId, string versionName, Func<PageRecord, PageVersionRecord, T> map);

        /// <summary>
        /// Retrieves a single layout version by its ID number
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="elementId">The unique ID of the layout to return</param>
        /// <param name="versionName">The name of the website version to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        T GetLayout<T>(long elementId, string versionName, Func<LayoutRecord, LayoutVersionRecord, T> map);

        /// <summary>
        /// Retrieves a list of matching pages for a specific version of the website
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="versionName">The name of the website version to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which pages to return. 
        /// If null is passed then all pages for the specified version are returned</param>
        IList<T> GetPages<T>(string versionName, Func<PageRecord, PageVersionRecord, T> map, Func<PageRecord, PageVersionRecord, bool> predicate = null);

        /// <summary>
        /// Retrieves a list of matching layouts for a specific version of the website
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="versionName">The name of the website version to get</param>
        /// <param name="map">A function that maps database records onto the return type</param>
        /// <param name="predicate">A function that determines which layouts to return. 
        /// If null is passed then all layouts for the specified version are returned</param>
        IList<T> GetLayouts<T>(string versionName, Func<LayoutRecord, LayoutVersionRecord, T> map, Func<LayoutRecord, LayoutVersionRecord, bool> predicate = null);
    }
}
