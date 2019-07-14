using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OwinFramework.Pages.CMS.Runtime.Data
{
    public class TestDatabaseReaderBase: IDatabaseReader
    {
        protected EnvironmentRecord[] _environments;

        protected WebsiteVersionRecord[] _websiteVersions;
        protected WebsiteVersionPageRecord[] _websiteVersionPages;
        protected WebsiteVersionLayoutRecord[] _websiteVersionLayouts;
        protected WebsiteVersionRegionRecord[] _websiteVersionRegions;
        protected WebsiteVersionDataTypeRecord[] _websiteVersionDataTypes;
        protected WebsiteVersionComponentRecord[] _websiteVersionComponents;

        protected PageRecord[] _pages;
        protected PageVersionRecord[] _pageVersions;

        protected LayoutRecord[] _layouts;
        protected LayoutVersionRecord[] _layoutVersions;

        protected RegionRecord[] _regions;
        protected RegionVersionRecord[] _regionVersions;

        protected ComponentRecord[] _components;
        protected ComponentVersionRecord[] _componentVersions;

        protected DataScopeRecord[] _dataScopes;
        protected DataTypeRecord[] _dataTypes;
        protected DataTypeVersionRecord[] _dataTypeVersions;

        protected ElementPropertyRecord[] _properties;
        protected ElementPropertyValueRecord[] _propertyValues;

        T[] IDatabaseReader.GetEnvironments<T>(Func<EnvironmentRecord, T> map, Func<EnvironmentRecord, bool> predicate)
        {
            if (predicate == null)
                return _environments
                    .Select(map)
                    .ToArray();

            return _environments
                .Where(predicate)
                .Select(map)
                .ToArray();
        }

        T[] IDatabaseReader.GetWebsiteVersions<T>(Func<WebsiteVersionRecord, T> map, Func<WebsiteVersionRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersions
                    .Select(map)
                    .ToArray();

            return _websiteVersions
                .Where(predicate)
                .Select(map)
                .ToArray();
        }

        T[] IDatabaseReader.GetWebsitePages<T>(
            long websiteVersionId, 
            Func<WebsiteVersionPageRecord, T> map,
            Func<WebsiteVersionPageRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersionPages
                    .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                    .Select(map)
                    .ToArray();

            return _websiteVersionPages
                .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                .Where(predicate)
                .Select(map)
                .ToArray();
        }

        T[] IDatabaseReader.GetWebsitePages<T>(
            string websiteVersionName, 
            Func<WebsiteVersionPageRecord, T> map,
            Func<WebsiteVersionPageRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsitePages(websiteVersion.RecordId, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteComponents<T>(long websiteVersionId, Func<WebsiteVersionComponentRecord, T> map, Func<WebsiteVersionComponentRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersionComponents
                    .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                    .Select(map)
                    .ToArray();

            return _websiteVersionComponents
                .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                .Where(predicate)
                .Select(map)
                .ToArray();
        }

        T[] IDatabaseReader.GetWebsiteComponents<T>(string websiteVersionName, Func<WebsiteVersionComponentRecord, T> map, Func<WebsiteVersionComponentRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteComponents(websiteVersion.RecordId, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteLayouts<T>(long websiteVersionId, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersionLayouts
                    .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                    .Select(map)
                    .ToArray();

            return _websiteVersionLayouts
                .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                .Where(predicate)
                .Select(map)
                .ToArray();
        }

        T[] IDatabaseReader.GetWebsiteLayouts<T>(string websiteVersionName, Func<WebsiteVersionLayoutRecord, T> map, Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteLayouts(websiteVersion.RecordId, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteRegions<T>(long websiteVersionId, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersionRegions
                    .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                    .Select(map)
                    .ToArray();

            return _websiteVersionRegions
                .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                .Where(predicate)
                .Select(map)
                .ToArray();
        }

        T[] IDatabaseReader.GetWebsiteRegions<T>(string websiteVersionName, Func<WebsiteVersionRegionRecord, T> map, Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteRegions(websiteVersion.RecordId, map, predicate);
        }

        
        T[] IDatabaseReader.GetWebsiteDataTypes<T>(long websiteVersionId, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate)
        {
            if (predicate == null)
                return _websiteVersionDataTypes
                    .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                    .Select(map)
                    .ToArray();

            return _websiteVersionDataTypes
                .Where(pv => pv.WebsiteVersionId == websiteVersionId)
                .Where(predicate)
                .Select(map)
                .ToArray();
        }

        T[] IDatabaseReader.GetWebsiteDataTypes<T>(string websiteVersionName, Func<WebsiteVersionDataTypeRecord, T> map, Func<WebsiteVersionDataTypeRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteDataTypes(websiteVersion.RecordId, map, predicate);
        }

        
        T[] IDatabaseReader.GetPages<T>(Func<PageRecord, T> map, Func<PageRecord, bool> predicate)
        {
            if (predicate == null)
                return _pages.Select(map).ToArray();

            return _pages.Where(predicate).Select(map).ToArray();
        }

        IDictionary<string, object> IDatabaseReader.GetElementPropertyValues(long elementVersionId)
        {
            return _propertyValues
                .Where(pv => pv.ParentRecordId == elementVersionId)
                .Select(pv =>
                    {
                        var property = _properties.FirstOrDefault(p => p.RecordId == pv.RecordId);
                        if (property == null)
                            throw new Exception("Property value on element version " + pv.ParentRecordId +
                                                " references non-existent property " + pv.RecordId);
                        return new {property.Name, pv.Value};
                    } )
                .ToDictionary(p => p.Name, p => p.Value);
        }

        T[] IDatabaseReader.GetElementVersions<T>(
            long elementId, 
            Func<ElementVersionRecordBase, T> map)
        {
            IEnumerable<ElementVersionRecordBase> elementVersions = null;

            if (elementVersions == null)
            {
                var page = _pages.FirstOrDefault(p => p.RecordId == elementId);
                if (page != null)
                {
                    elementVersions = _pageVersions.Where(pv => pv.ParentRecordId == page.RecordId);
                }
            }

            if (elementVersions == null)
            {
                var layout = _layouts.FirstOrDefault(l => l.RecordId == elementId);
                if (layout != null)
                {
                    elementVersions = _layoutVersions.Where(lv => lv.ParentRecordId == layout.RecordId);
                }
            }
            
            if (elementVersions == null)
            {
                var region = _regions.FirstOrDefault(r => r.RecordId == elementId);
                if (region != null)
                {
                    elementVersions = _layoutVersions.Where(lv => lv.ParentRecordId == region.RecordId);
                }
            }
            
            return elementVersions == null 
                ? new T[0]
                : elementVersions.Select(map).ToArray();
        }

        T IDatabaseReader.GetEnvironment<T>(long environmentId, Func<EnvironmentRecord, T> map)
        {
            var environmant = _environments.FirstOrDefault(e => e.RecordId == environmentId);
            if (environmant == null) return default(T);
            return map(environmant);
        }

        T IDatabaseReader.GetWebsiteVersion<T>(long websiteVersionId, Func<WebsiteVersionRecord, T> map)
        {
            var websiteVersion = _websiteVersions.FirstOrDefault(e => e.RecordId == websiteVersionId);
            if (websiteVersion == null) return default(T);
            return map(websiteVersion);
        }

        T IDatabaseReader.GetPageVersion<T>(long pageId, int version, Func<PageRecord, PageVersionRecord, T> map)
        {
            var page = _pages.FirstOrDefault(p => p.RecordId == pageId);
            if (page == null) return default(T);

            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.ParentRecordId == page.RecordId && pv.Version == version);
            if (pageVersion == null) return default(T);

            return map(page, pageVersion);
        }

        T IDatabaseReader.GetLayoutVersion<T>(long layoutId, int version, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            var layout = _layouts.FirstOrDefault(l => l.RecordId == layoutId);
            if (layout == null) return default(T);

            var layoutVersion = _layoutVersions.FirstOrDefault(lv => lv.ParentRecordId == layout.RecordId && lv.Version == version);
            if (layoutVersion == null) return default(T);

            return map(layout, layoutVersion);
        }

        T IDatabaseReader.GetRegionVersion<T>(long regionId, int version, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            var region = _regions.FirstOrDefault(r => r.RecordId == regionId);
            if (region == null) return default(T);

            var regionVersion = _regionVersions.FirstOrDefault(rv => rv.ParentRecordId == region.RecordId && rv.Version == version);
            if (regionVersion == null) return default(T);

            return map(region, regionVersion);
        }

        T IDatabaseReader.GetPageVersion<T>(long pageVersionId, Func<PageRecord, PageVersionRecord, T> map)
        {
            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.RecordId == pageVersionId);
            if (pageVersion == null) return default(T);

            var page = _pages.FirstOrDefault(p => p.RecordId == pageVersion.ParentRecordId);
            if (page == null) return default(T);

            return map(page, pageVersion);
        }

        T IDatabaseReader.GetLayoutVersion<T>(long layoutVersionId, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            var layoutVersion = _layoutVersions.FirstOrDefault(lv => lv.RecordId == layoutVersionId);
            if (layoutVersion == null) return default(T);

            var layout = _layouts.FirstOrDefault(l => l.RecordId == layoutVersion.ParentRecordId);
            if (layout == null) return default(T);

            return map(layout, layoutVersion);
        }

        T IDatabaseReader.GetRegionVersion<T>(long regionVersionId, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            var regionVersion = _regionVersions.FirstOrDefault(lv => lv.RecordId == regionVersionId);
            if (regionVersion == null) return default(T);

            var region = _regions.FirstOrDefault(r => r.RecordId == regionVersion.ParentRecordId);
            if (region == null) return default(T);

            return map(region, regionVersion);
        }

        T IDatabaseReader.GetComponentVersion<T>(long componentVersionId, Func<ComponentRecord, ComponentVersionRecord, T> map)
        {
            var componentVersion = _componentVersions.FirstOrDefault(lv => lv.RecordId == componentVersionId);
            if (componentVersion == null) return default(T);

            var component = _components.FirstOrDefault(r => r.RecordId == componentVersion.ParentRecordId);
            if (component == null) return default(T);

            return map(component, componentVersion);
        }

        T IDatabaseReader.GetDataTypeVersion<T>(long dataTypeVersionId, Func<DataTypeRecord, DataTypeVersionRecord, T> map)
        {
            var dataTypeVersion = _dataTypeVersions.FirstOrDefault(v => v.RecordId == dataTypeVersionId);
            if (dataTypeVersion == null) return default(T);

            var dataType = _dataTypes.FirstOrDefault(r => r.RecordId == dataTypeVersion.ParentRecordId);
            if (dataType == null) return default(T);

            dataTypeVersion.ScopeIds = _dataScopes
                .Where(s => s.DataTypeId == dataType.RecordId)
                .Select(r => r.RecordId)
                .ToArray();

            return map(dataType, dataTypeVersion);
        }

        HistoryPeriodRecord IDatabaseReader.GetHistory(string recordType, long id, string bookmark)
        {
            return new HistoryPeriodRecord
            {
                RecordType = recordType,
                RecordId = id,
                EndDateTime = DateTime.UtcNow,
            };
        }

        HistoryEventRecord[] IDatabaseReader.GetHistorySummary(long summaryId)
        {
            return null;
        }
    }
}
