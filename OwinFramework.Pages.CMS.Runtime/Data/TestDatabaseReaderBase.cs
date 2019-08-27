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
        protected ModuleRecord[] _modules;

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

        protected HistoryEventRecord[] _historyEvents;
        protected HistoryPeriodRecord[] _historyPeriods;

        T[] IDatabaseReader.GetEnvironments<T>(Func<EnvironmentRecord, T> map, Func<EnvironmentRecord, bool> predicate)
        {
            var environments = predicate == null
                ? _environments
                : _environments.Where(predicate);

            return environments
                .Select(map)
                .ToArray();
        }

        T[] IDatabaseReader.GetWebsiteVersions<T>(Func<WebsiteVersionRecord, T> map, Func<WebsiteVersionRecord, bool> predicate)
        {
            var versions = predicate == null
                ? _websiteVersions
                : _websiteVersions.Where(predicate);

            return versions
                .Select(map)
                .ToArray();
        }

        T[] IDatabaseReader.GetWebsitePages<T>(
            long websiteVersionId, 
            string scenarioName,
            Func<WebsiteVersionPageRecord, T> map,
            Func<WebsiteVersionPageRecord, bool> predicate)
        {
            return GetWebsiteVersionElements(
                _websiteVersionPages, 
                websiteVersionId,
                scenarioName,
                map,
                predicate);
        }

        T[] IDatabaseReader.GetWebsitePages<T>(
            string websiteVersionName, 
            string scenarioName,
            Func<WebsiteVersionPageRecord, T> map,
            Func<WebsiteVersionPageRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsitePages(websiteVersion.RecordId, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteComponents<T>(
            long websiteVersionId, 
            string scenarioName,
            Func<WebsiteVersionComponentRecord, T> map, 
            Func<WebsiteVersionComponentRecord, bool> predicate)
        {
            return GetWebsiteVersionElements(
                _websiteVersionComponents, 
                websiteVersionId,
                scenarioName,
                map,
                predicate);
        }

        T[] IDatabaseReader.GetWebsiteComponents<T>(
            string websiteVersionName, 
            string scenarioName,
            Func<WebsiteVersionComponentRecord, T> map, 
            Func<WebsiteVersionComponentRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteComponents(websiteVersion.RecordId, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteLayouts<T>(
            long websiteVersionId, 
            string scenarioName,
            Func<WebsiteVersionLayoutRecord, T> map, 
            Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            return GetWebsiteVersionElements(
                _websiteVersionLayouts, 
                websiteVersionId,
                scenarioName,
                map,
                predicate);
        }

        T[] IDatabaseReader.GetWebsiteLayouts<T>(
            string websiteVersionName, 
            string scenarioName,
            Func<WebsiteVersionLayoutRecord, T> map, 
            Func<WebsiteVersionLayoutRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteLayouts(websiteVersion.RecordId, scenarioName, map, predicate);
        }

        T[] IDatabaseReader.GetWebsiteRegions<T>(
            long websiteVersionId, 
            string scenarioName,
            Func<WebsiteVersionRegionRecord, T> map, 
            Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            return GetWebsiteVersionElements(
                _websiteVersionRegions, 
                websiteVersionId,
                scenarioName,
                map,
                predicate);
        }

        T[] IDatabaseReader.GetWebsiteRegions<T>(
            string websiteVersionName, 
            string scenarioName,
            Func<WebsiteVersionRegionRecord, T> map, 
            Func<WebsiteVersionRegionRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteRegions(websiteVersion.RecordId, scenarioName, map, predicate);
        }

        
        T[] IDatabaseReader.GetWebsiteDataTypes<T>(
            long websiteVersionId, 
            string scenarioName,
            Func<WebsiteVersionDataTypeRecord, T> map, 
            Func<WebsiteVersionDataTypeRecord, bool> predicate)
        {
            return GetWebsiteVersionElements(
                _websiteVersionDataTypes, 
                websiteVersionId,
                scenarioName,
                map,
                predicate);
        }

        T[] IDatabaseReader.GetWebsiteDataTypes<T>(
            string websiteVersionName, 
            string scenarioName,
            Func<WebsiteVersionDataTypeRecord, T> map, 
            Func<WebsiteVersionDataTypeRecord, bool> predicate)
        {
            var websiteVersion = _websiteVersions
                .FirstOrDefault(v => string.Equals(v.Name, websiteVersionName, StringComparison.OrdinalIgnoreCase));

            return websiteVersion == null 
                ? null 
                : ((IDatabaseReader)this).GetWebsiteDataTypes(websiteVersion.RecordId, scenarioName, map, predicate);
        }

        
        T[] IDatabaseReader.GetPages<T>(Func<PageRecord, T> map, Func<PageRecord, bool> predicate)
        {
            return (predicate == null ? _pages : _pages.Where(predicate)).Select(map).ToArray();
        }

        T[] IDatabaseReader.GetLayouts<T>(Func<LayoutRecord, T> map, Func<LayoutRecord, bool> predicate)
        {
            return (predicate == null ? _layouts : _layouts.Where(predicate)).Select(map).ToArray();
        }

        T[] IDatabaseReader.GetRegions<T>(Func<RegionRecord, T> map, Func<RegionRecord, bool> predicate)
        {
            return (predicate == null ? _regions : _regions.Where(predicate)).Select(map).ToArray();
        }

        T[] IDatabaseReader.GetDataTypes<T>(Func<DataTypeRecord, T> map, Func<DataTypeRecord, bool> predicate)
        {
            return (predicate == null ? _dataTypes : _dataTypes.Where(predicate)).Select(map).ToArray();
        }

        T[] IDatabaseReader.GetDataScopes<T>(Func<DataScopeRecord, T> map, Func<DataScopeRecord, bool> predicate)
        {
            return (predicate == null ? _dataScopes : _dataScopes.Where(predicate)).Select(map).ToArray();
        }

        T[] IDatabaseReader.GetComponents<T>(Func<ComponentRecord, T> map, Func<ComponentRecord, bool> predicate)
        {
            return (predicate == null ? _components : _components.Where(predicate)).Select(map).ToArray();
        }

        T[] IDatabaseReader.GetModules<T>(Func<ModuleRecord, T> map, Func<ModuleRecord, bool> predicate)
        {
            return (predicate == null ? _modules : _modules.Where(predicate)).Select(map).ToArray();
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

        private T2[] GetWebsiteVersionElements<T1, T2>(
            T1[] elements,
            long websiteVersionId, 
            string scenarioName,
            Func<T1, T2> map, 
            Func<T1, bool> predicate) where T1: WebsiteVersionRecordBase
        {
            Func<T1, bool> where = pv => 
                pv.WebsiteVersionId == websiteVersionId &&
                (
                    (string.IsNullOrEmpty(scenarioName) && string.IsNullOrEmpty(pv.Scenario)) ||
                    string.Equals(pv.Scenario, scenarioName, StringComparison.OrdinalIgnoreCase)
                    ) &&
                (predicate == null || predicate(pv));

            return elements
                .Where(where)
                .Select(map)
                .ToArray();
            
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
            if (environmant == null) return default;
            return map(environmant);
        }

        T IDatabaseReader.GetWebsiteVersion<T>(long websiteVersionId, Func<WebsiteVersionRecord, T> map)
        {
            var websiteVersion = _websiteVersions.FirstOrDefault(e => e.RecordId == websiteVersionId);
            if (websiteVersion == null) return default;
            return map(websiteVersion);
        }

        T IDatabaseReader.GetPage<T>(long pageId, Func<PageRecord, T> map)
        {
            var page = _pages.FirstOrDefault(p => p.RecordId == pageId);
            return page == null ? default : map(page);
        }

        T IDatabaseReader.GetLayout<T>(long layoutId, Func<LayoutRecord, T> map)
        {
            var layout = _layouts.FirstOrDefault(p => p.RecordId == layoutId);
            return layout == null ? default : map(layout);
        }

        T IDatabaseReader.GetRegion<T>(long regionId, Func<RegionRecord, T> map)
        {
            var region = _regions.FirstOrDefault(p => p.RecordId == regionId);
            return region == null ? default : map(region);
        }

        T IDatabaseReader.GetDataScope<T>(long dataScopeId, Func<DataScopeRecord, T> map)
        {
            var dataScope = _dataScopes.FirstOrDefault(p => p.RecordId == dataScopeId);
            return dataScope == null ? default : map(dataScope);
        }

        T IDatabaseReader.GetDataType<T>(long dataTypeId, Func<DataTypeRecord, T> map)
        {
            var dataTypes = _dataTypes.FirstOrDefault(p => p.RecordId == dataTypeId);
            return dataTypes == null ? default : map(dataTypes);
        }

        T IDatabaseReader.GetPageVersion<T>(long pageId, int version, Func<PageRecord, PageVersionRecord, T> map)
        {
            var page = _pages.FirstOrDefault(p => p.RecordId == pageId);
            if (page == null) return default;

            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.ParentRecordId == page.RecordId && pv.Version == version);
            if (pageVersion == null) return default;

            return map(page, pageVersion);
        }

        T IDatabaseReader.GetLayoutVersion<T>(long layoutId, int version, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            var layout = _layouts.FirstOrDefault(l => l.RecordId == layoutId);
            if (layout == null) return default;

            var layoutVersion = _layoutVersions.FirstOrDefault(lv => lv.ParentRecordId == layout.RecordId && lv.Version == version);
            if (layoutVersion == null) return default;

            return map(layout, layoutVersion);
        }

        T IDatabaseReader.GetRegionVersion<T>(long regionId, int version, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            var region = _regions.FirstOrDefault(r => r.RecordId == regionId);
            if (region == null) return default;

            var regionVersion = _regionVersions.FirstOrDefault(rv => rv.ParentRecordId == region.RecordId && rv.Version == version);
            if (regionVersion == null) return default;

            return map(region, regionVersion);
        }

        T IDatabaseReader.GetDataTypeVersion<T>(long dataTypeId, int version, Func<DataTypeRecord, DataTypeVersionRecord, T> map)
        {
            var dataType = _dataTypes.FirstOrDefault(r => r.RecordId == dataTypeId);
            if (dataType == null) return default;

            var dataTypeVersion = _dataTypeVersions.FirstOrDefault(rv => rv.ParentRecordId == dataType.RecordId && rv.Version == version);
            if (dataTypeVersion == null) return default;

            return map(dataType, dataTypeVersion);
        }

        T IDatabaseReader.GetPageVersion<T>(long pageVersionId, Func<PageRecord, PageVersionRecord, T> map)
        {
            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.RecordId == pageVersionId);
            if (pageVersion == null) return default;

            var page = _pages.FirstOrDefault(p => p.RecordId == pageVersion.ParentRecordId);
            if (page == null) return default;

            return map(page, pageVersion);
        }

        T IDatabaseReader.GetLayoutVersion<T>(long layoutVersionId, Func<LayoutRecord, LayoutVersionRecord, T> map)
        {
            var layoutVersion = _layoutVersions.FirstOrDefault(lv => lv.RecordId == layoutVersionId);
            if (layoutVersion == null) return default;

            var layout = _layouts.FirstOrDefault(l => l.RecordId == layoutVersion.ParentRecordId);
            if (layout == null) return default;

            return map(layout, layoutVersion);
        }

        T IDatabaseReader.GetRegionVersion<T>(long regionVersionId, Func<RegionRecord, RegionVersionRecord, T> map)
        {
            var regionVersion = _regionVersions.FirstOrDefault(lv => lv.RecordId == regionVersionId);
            if (regionVersion == null) return default;

            var region = _regions.FirstOrDefault(r => r.RecordId == regionVersion.ParentRecordId);
            if (region == null) return default;

            return map(region, regionVersion);
        }

        T IDatabaseReader.GetComponentVersion<T>(long componentVersionId, Func<ComponentRecord, ComponentVersionRecord, T> map)
        {
            var componentVersion = _componentVersions.FirstOrDefault(lv => lv.RecordId == componentVersionId);
            if (componentVersion == null) return default;

            var component = _components.FirstOrDefault(r => r.RecordId == componentVersion.ParentRecordId);
            if (component == null) return default;

            return map(component, componentVersion);
        }

        T IDatabaseReader.GetDataTypeVersion<T>(long dataTypeVersionId, Func<DataTypeRecord, DataTypeVersionRecord, T> map)
        {
            var dataTypeVersion = _dataTypeVersions.FirstOrDefault(v => v.RecordId == dataTypeVersionId);
            if (dataTypeVersion == null) return default;

            var dataType = _dataTypes.FirstOrDefault(r => r.RecordId == dataTypeVersion.ParentRecordId);
            if (dataType == null) return default;

            dataTypeVersion.ScopeIds = _dataScopes
                .Where(s => s.DataTypeId == dataType.RecordId)
                .Select(r => r.RecordId)
                .ToArray();

            return map(dataType, dataTypeVersion);
        }

        HistoryPeriodRecord IDatabaseReader.GetHistory(string recordType, long id, string bookmark)
        {
            if (_historyPeriods == null) return null;

            return _historyPeriods
                .Where(p => string.Equals(p.RecordType, recordType, StringComparison.OrdinalIgnoreCase))
                .Where(p => p.RecordId == id)
                .OrderByDescending(p => p.EndDateTime)
                .FirstOrDefault();
        }

        HistoryEventRecord[] IDatabaseReader.GetHistorySummary(long summaryId)
        {
            if (_historyEvents == null) return null;
            return _historyEvents.Where(e => e.SummaryId == summaryId).ToArray();
        }

        T[] IDatabaseReader.GetElementUsage<T>(long elementVersionId, Func<WebsiteVersionRecordBase, T> map, Func<WebsiteVersionRecordBase, bool> predicate)
        {
            if (predicate == null) predicate = wv => true;

            return Enumerable.Empty<WebsiteVersionRecordBase>()
                .Concat(_websiteVersionPages.Where(p => p.PageVersionId == elementVersionId).Cast<WebsiteVersionRecordBase>())
                .Concat(_websiteVersionLayouts.Where(p => p.LayoutVersionId == elementVersionId).Cast<WebsiteVersionRecordBase>())
                .Concat(_websiteVersionRegions.Where(p => p.RegionVersionId == elementVersionId).Cast<WebsiteVersionRecordBase>())
                .Concat(_websiteVersionDataTypes.Where(p => p.DataTypeVersionId == elementVersionId).Cast<WebsiteVersionRecordBase>())
                .Concat(_websiteVersionComponents.Where(p => p.ComponentVersionId == elementVersionId).Cast<WebsiteVersionRecordBase>())
                .Where(predicate)
                .Select(map)
                .ToArray();
        }
    }
}
