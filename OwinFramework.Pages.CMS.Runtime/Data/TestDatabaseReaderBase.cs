using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OwinFramework.Pages.CMS.Runtime.Data
{
    public class TestDatabaseReaderBase: IDatabaseReader
    {
        protected List<WebsiteVersionRecord> _websiteVersions;
        protected List<WebsiteVersionPageRecord> _websiteVersionPages;
        protected List<WebsiteVersionLayoutRecord> _websiteVersionLayouts;
        protected List<WebsiteVersionRegionRecord> _websiteVersionRegions;

        protected List<PageRecord> _pages;
        protected List<PageVersionRecord> _pageVersions;

        protected List<LayoutRecord> _layouts;
        protected List<LayoutVersionRecord> _layoutVersions;

        protected List<RegionRecord> _regions;
        protected List<RegionVersionRecord> _regionVersions;

        protected List<DataScopeRecord> _dataScopes;
        protected List<DataTypeRecord> _dataTypes;
        protected List<DataTypeVersionRecord> _dataTypeVersions;

        protected List<ElementPropertyRecord> _properties;

        IList<T> IDatabaseReader.GetWebsiteVersions<T>(Func<WebsiteVersionRecord, T> map, Func<WebsiteVersionRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersions
                    .Select(map)
                    .ToList();

            return _websiteVersions
                .Where(predicate)
                .Select(map)
                .ToList();
        }

        IList<T> IDatabaseReader.GetWebsiteVersionPages<T>(
            long websiteVersionId, 
            Func<WebsiteVersionPageRecord, T> map,
            Func<WebsiteVersionPageRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersionPages
                    .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                    .Select(map)
                    .ToList();

            return _websiteVersionPages
                .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                .Where(predicate)
                .Select(map)
                .ToList();
        }

        IList<T> IDatabaseReader.GetWebsiteVersionPages<T>(
            string websiteVersionName, 
            Func<WebsiteVersionPageRecord, T> map,
            Func<WebsiteVersionPageRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteVersionPages(websiteVersion.Id, map, predicate);
        }

        IList<T> IDatabaseReader.GetWebsiteVersionLayouts<T>(long websiteVersionId, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersionLayouts
                    .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                    .Select(map)
                    .ToList();

            return _websiteVersionLayouts
                .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                .Where(predicate)
                .Select(map)
                .ToList();
        }

        IList<T> IDatabaseReader.GetWebsiteVersionLayouts<T>(string websiteVersionName, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteVersionLayouts(websiteVersion.Id, map, predicate);
        }

        IList<T> IDatabaseReader.GetWebsiteVersionRegions<T>(long websiteVersionId, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersionRegions
                    .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                    .Select(map)
                    .ToList();

            return _websiteVersionRegions
                .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                .Where(predicate)
                .Select(map)
                .ToList();
        }

        IList<T> IDatabaseReader.GetWebsiteVersionRegions<T>(string websiteVersionName, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteVersionRegions(websiteVersion.Id, map, predicate);
        }

        
        IDictionary<string, string> IDatabaseReader.GetElementProperties(long elementVersionId)
        {
            return _properties
                .Where(p => p.ElementVersionId == elementVersionId)
                .ToDictionary(p => p.Name, p => p.Value);
        }

        IList<T> IDatabaseReader.GetElementVersions<T>(
            long elementId, 
            Func<ElementVersionRecordBase, T> map)
        {
            IEnumerable<ElementVersionRecordBase> elementVersions = null;

            if (elementVersions == null)
            {
                var page = _pages.FirstOrDefault(p => p.ElementId == elementId);
                if (page != null)
                {
                    elementVersions = _pageVersions.Where(pv => pv.ElementId == page.ElementId);
                }
            }

            if (elementVersions == null)
            {
                var layout = _layouts.FirstOrDefault(l => l.ElementId == elementId);
                if (layout != null)
                {
                    elementVersions = _layoutVersions.Where(lv => lv.ElementId == layout.ElementId);
                }
            }
            
            if (elementVersions == null)
            {
                var region = _regions.FirstOrDefault(r => r.ElementId == elementId);
                if (region != null)
                {
                    elementVersions = _layoutVersions.Where(lv => lv.ElementId == region.ElementId);
                }
            }
            
            return elementVersions == null 
                ? new List<T>()
                : elementVersions.Select(map).ToList();
        }

        T IDatabaseReader.GetPage<T>(long pageId, int version, Func<PageRecord, PageVersionRecord, T> map)
        {
            var page = _pages.FirstOrDefault(p => p.ElementId == pageId);
            if (page == null) return default(T);

            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.ElementId == page.ElementId && pv.Version == version);
            if (pageVersion == null) return default(T);

            return map(page, pageVersion);
        }

        T IDatabaseReader.GetLayout<T>(long layoutId, int version, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            var layout = _layouts.FirstOrDefault(l => l.ElementId == layoutId);
            if (layout == null) return default(T);

            var layoutVersion = _layoutVersions.FirstOrDefault(lv => lv.ElementId == layout.ElementId && lv.Version == version);
            if (layoutVersion == null) return default(T);

            return map(layout, layoutVersion);
        }

        T IDatabaseReader.GetRegion<T>(long regionId, int version, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            var region = _regions.FirstOrDefault(r => r.ElementId == regionId);
            if (region == null) return default(T);

            var regionVersion = _regionVersions.FirstOrDefault(rv => rv.ElementId == region.ElementId && rv.Version == version);
            if (regionVersion == null) return default(T);

            return map(region, regionVersion);
        }

        T IDatabaseReader.GetPage<T>(long pageVersionId, Func<PageRecord, PageVersionRecord, T> map)
        {
            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.ElementVersionId == pageVersionId);
            if (pageVersion == null) return default(T);

            var page = _pages.FirstOrDefault(p => p.ElementId == pageVersion.ElementId);
            if (page == null) return default(T);

            return map(page, pageVersion);
        }

        T IDatabaseReader.GetLayout<T>(long layoutVersionId, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            var layoutVersion = _layoutVersions.FirstOrDefault(lv => lv.ElementVersionId == layoutVersionId);
            if (layoutVersion == null) return default(T);

            var layout = _layouts.FirstOrDefault(l => l.ElementId == layoutVersion.ElementId);
            if (layout == null) return default(T);

            return map(layout, layoutVersion);
        }

        T IDatabaseReader.GetRegion<T>(long regionVersionId, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            var regionVersion = _regionVersions.FirstOrDefault(lv => lv.ElementVersionId == regionVersionId);
            if (regionVersion == null) return default(T);

            var region = _regions.FirstOrDefault(r => r.ElementId == regionVersion.ElementId);
            if (region == null) return default(T);

            return map(region, regionVersion);
        }
    }
}
