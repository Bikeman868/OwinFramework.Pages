using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OwinFramework.Pages.CMS.Runtime.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.CMS.Manager.Data
{
    public class TestDatabaseUpdaterBase : TestDatabaseReaderBase, IDatabaseUpdater
    {
        private readonly object _updateLock = new object();
        private long _nextHistoryEventId;
        private long _nextHistorySummaryId;

        private DynamicCast<PageRouteRecord> _pageRouteCast = new DynamicCast<PageRouteRecord>();
        private DynamicCast<ElementComponentRecord> _elementComponentCast = new DynamicCast<ElementComponentRecord>();
        private DynamicCast<ElementDataScopeRecord> _elementDataScopeCast = new DynamicCast<ElementDataScopeRecord>();
        private DynamicCast<ElementDataTypeRecord> _elementDataTypeCast = new DynamicCast<ElementDataTypeRecord>();
        private DynamicCast<LayoutZoneRecord> _layoutZoneCast = new DynamicCast<LayoutZoneRecord>();
        private DynamicCast<RegionTemplateRecord> _regionTemplateCast = new DynamicCast<RegionTemplateRecord>();

        #region History

        protected HistoryEventRecord AddHistory(
            RecordBase record,
            string identity,
            params HistoryChangeDetails[] details)
        {
            HistoryPeriodRecord period;
            lock (_updateLock)
            {
                if (_historyPeriods == null)
                {
                    period = new HistoryPeriodRecord
                    {
                        RecordType = record.RecordType,
                        RecordId = record.RecordId,
                        StartDateTime = DateTime.UtcNow,
                    };
                    _historyPeriods = new[] { period };
                }
                else
                {
                    period = _historyPeriods.FirstOrDefault(p =>
                        p.RecordId == record.RecordId &&
                        string.Equals(p.RecordType, record.RecordType, StringComparison.OrdinalIgnoreCase));
                    if (period == null)
                    {
                        period = new HistoryPeriodRecord
                        {
                            RecordType = record.RecordType,
                            RecordId = record.RecordId,
                            StartDateTime = DateTime.UtcNow,
                        };
                        var list = _historyPeriods.ToList();
                        list.Add(period);
                        _historyPeriods = list.ToArray();
                    }
                }
            }
            period.EndDateTime = DateTime.UtcNow;

            HistorySummaryRecord summary;
            lock (_updateLock)
            {
                if (period.Summaries == null)
                {
                    summary = new HistorySummaryRecord
                    {
                        SummaryId = Interlocked.Increment(ref _nextHistorySummaryId),
                        Identity = identity,
                        When = DateTime.UtcNow
                    };
                    period.Summaries = new[] { summary };
                }
                else
                {
                    summary = period.Summaries.FirstOrDefault(s => s.Identity == identity);
                    if (summary == null)
                    {
                        summary = new HistorySummaryRecord
                        {
                            SummaryId = Interlocked.Increment(ref _nextHistorySummaryId),
                            Identity = identity,
                            When = DateTime.UtcNow
                        };
                        var list = period.Summaries.ToList();
                        list.Add(summary);
                        period.Summaries = list.ToArray();
                    }
                }
            }

            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include
            };

            var historyEvent = new HistoryEventRecord
            {
                EventId = Interlocked.Increment(ref _nextHistoryEventId),
                When = DateTime.UtcNow,
                RecordType = record.RecordType,
                RecordId = record.RecordId,
                Identity = identity,
                SummaryId = summary.SummaryId,
                ChangeDetails = JsonConvert.SerializeObject(
                    details,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Include
                    })
            };

            lock (_updateLock)
            {
                if (_historyEvents == null)
                    _historyEvents = new[] { historyEvent };
                else
                {
                    var list = _historyEvents.ToList();
                    list.Add(historyEvent);
                    _historyEvents = list.ToArray();
                }
            }

            summary.ChangeSummary = _historyEvents.Count(e => e.SummaryId == summary.SummaryId) + " changes";

            return historyEvent;
        }

        #endregion

        #region Environments

        CreateResult IDatabaseUpdater.CreateEnvironment(string identity, EnvironmentRecord environment)
        {
            lock (_updateLock)
            {
                environment.RecordId = _environments.OrderByDescending(e => e.RecordId).First().RecordId + 1;
                environment.CreatedWhen = DateTime.UtcNow;
                environment.CreatedBy = identity;

                var environments = _environments.ToList();
                environments.Add(environment);
                _environments = environments.ToArray();

                AddHistory(
                    environment,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                return new CreateResult(environment.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdateEnvironment(string identity, long environmentId, IEnumerable<PropertyChange> changes)
        {
            var environment = _environments.FirstOrDefault(p => p.RecordId == environmentId);
            if (environment == null) return new UpdateResult("environment_not_found", "No environment found with id " + environmentId);

            var details = new List<HistoryChangeDetails>();

            foreach (var change in changes)
            {
                var changeDetails = new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = change.PropertyName,
                    NewValue = change.PropertyValue
                };

                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        changeDetails.OldValue = environment.Name;
                        environment.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        changeDetails.OldValue = environment.DisplayName;
                        environment.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        changeDetails.OldValue = environment.Description;
                        environment.Description = change.PropertyValue;
                        break;
                    case "baseurl":
                        changeDetails.OldValue = environment.BaseUrl;
                        environment.BaseUrl = change.PropertyValue;
                        break;
                    case "websiteversionid":
                        // TODO: record website version name
                        changeDetails.OldValue = environment.WebsiteVersionId.ToString();
                        environment.WebsiteVersionId = long.Parse(change.PropertyValue);
                        break;
                }
                details.Add(changeDetails);
            }

            AddHistory(environment, identity, details.ToArray());

            return new UpdateResult();
        }

        DeleteResult IDatabaseUpdater.DeleteEnvironment(string identity, long environmentId)
        {
            var environment = _environments.FirstOrDefault(p => p.RecordId == environmentId);
            if (environment == null) return new DeleteResult("environment_not_found", "No environment found with id " + environmentId);

            lock (_updateLock)
            {
                _environments = _environments.Where(p => p.RecordId != environmentId).ToArray();
            }

            AddHistory(
                environment,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Deleted"
                });

            return new DeleteResult();
        }

        #endregion

        #region Website Versions

        CreateResult IDatabaseUpdater.CreateWebsiteVersion(string identity, WebsiteVersionRecord websiteVersion)
        {
            lock (_updateLock)
            {
                websiteVersion.RecordId = _websiteVersions.OrderByDescending(e => e.RecordId).First().RecordId + 1;
                websiteVersion.CreatedWhen = DateTime.UtcNow;
                websiteVersion.CreatedBy = identity;

                var websiteVersions = _websiteVersions.ToList();
                websiteVersions.Add(websiteVersion);
                _websiteVersions = websiteVersions.ToArray();

                AddHistory(
                    websiteVersion,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                return new CreateResult(websiteVersion.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdateWebsiteVersion(string identity, long websiteVersionId, IEnumerable<PropertyChange> changes)
        {
            var websiteVersion = _websiteVersions.FirstOrDefault(p => p.RecordId == websiteVersionId);
            if (websiteVersion == null) return new UpdateResult("website_version_not_found", "No website version found with id " + websiteVersionId);

            var details = new List<HistoryChangeDetails>();

            foreach (var change in changes)
            {
                var changeDetails = new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = change.PropertyName,
                    NewValue = change.PropertyValue
                };

                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        changeDetails.OldValue = websiteVersion.Name;
                        websiteVersion.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        changeDetails.OldValue = websiteVersion.DisplayName;
                        websiteVersion.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        changeDetails.OldValue = websiteVersion.Description;
                        websiteVersion.Description = change.PropertyValue;
                        break;
                }
                details.Add(changeDetails);
            }

            AddHistory(websiteVersion, identity, details.ToArray());

            return new UpdateResult();
        }

        DeleteResult IDatabaseUpdater.DeleteWebsiteVersion(string identity, long websiteVersionId)
        {
            var websiteVersion = _websiteVersions.FirstOrDefault(p => p.RecordId == websiteVersionId);
            if (websiteVersion == null) return new DeleteResult("website_version_not_found", "No website version found with id " + websiteVersionId);

            lock (_updateLock)
            {
                _websiteVersions = _websiteVersions.Where(p => p.RecordId != websiteVersionId).ToArray();
            }

            AddHistory(
                websiteVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Deleted"
                });

            return new DeleteResult();
        }

        #endregion

        #region Pages

        CreateResult IDatabaseUpdater.CreatePage(string identity, PageRecord page)
        {
            lock (_updateLock)
            {
                page.RecordId = _pages.OrderByDescending(p => p.RecordId).First().RecordId + 1;
                page.CreatedWhen = DateTime.UtcNow;
                page.CreatedBy = identity;

                var pages = _pages.ToList();
                pages.Add(page);
                _pages = pages.ToArray();

                AddHistory(
                    page,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                var pageVersion = new PageVersionRecord
                {
                    RecordId = _pageVersions.OrderByDescending(pv => pv.RecordId).First().RecordId + 1,
                    CreatedWhen = page.CreatedWhen,
                    CreatedBy = page.CreatedBy,
                    ParentRecordId = page.RecordId,
                    Version = 1
                };

                var pageVersions = _pageVersions.ToList();
                pageVersions.Add(pageVersion);
                _pageVersions = pageVersions.ToArray();

                AddHistory(
                    pageVersion,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                return new CreateResult(page.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdatePage(string identity, long pageId, IEnumerable<PropertyChange> changes)
        {
            var page = _pages.FirstOrDefault(p => p.RecordId == pageId);
            if (page == null) return new UpdateResult("page_not_found", "No page found with id " + pageId);

            var details = new List<HistoryChangeDetails>();

            foreach (var change in changes)
            {
                var changeDetails = new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = change.PropertyName,
                    NewValue = change.PropertyValue
                };

                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        changeDetails.OldValue = page.Name;
                        page.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        changeDetails.OldValue = page.DisplayName;
                        page.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        changeDetails.OldValue = page.Description;
                        page.Description = change.PropertyValue;
                        break;
                }
                details.Add(changeDetails);
            }

            AddHistory(page, identity, details.ToArray());

            return new UpdateResult();
        }

        DeleteResult IDatabaseUpdater.DeletePage(string identity, long pageId)
        {
            var page = _pages.FirstOrDefault(p => p.RecordId == pageId);
            if (page == null) return new DeleteResult("page_not_found", "No page found with id " + pageId);

            lock (_updateLock)
            {
                _pages = _pages.Where(p => p.RecordId != pageId).ToArray();
            }

            AddHistory(
                page,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Deleted"
                });

            return new DeleteResult();
        }

        UpdateResult IDatabaseUpdater.AddPageToWebsiteVersion(string identity, long pageId, int version, long websiteVersionId, string scenario)
        {
            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.ParentRecordId == pageId && pv.Version == version);
            if (pageVersion == null)
            {
                return new UpdateResult(
                    "page_version_not_found",
                    "There is no version " + version + " of page id " + pageId);
            }

            AddPageVersionToWebsiteVersion(identity, pageVersion, websiteVersionId, scenario);

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.AddPageToWebsiteVersion(string identity, long pageVersionId, long websiteVersionId, string scenario)
        {
            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.RecordId == pageVersionId);
            if (pageVersion == null)
            {
                return new UpdateResult(
                    "page_version_not_found",
                    "There is page version " + pageVersionId);
            }

            AddPageVersionToWebsiteVersion(identity, pageVersion, websiteVersionId, scenario);

            return new UpdateResult();
        }

        private void AddPageVersionToWebsiteVersion(string identity, PageVersionRecord pageVersion, long websiteVersionId, string scenario)
        {
            IList<WebsiteVersionPageRecord> removedList;

            lock (_updateLock)
            {
                removedList = _websiteVersionPages
                    .Where(p =>
                        p.WebsiteVersionId == websiteVersionId &&
                        p.PageId == pageVersion.ParentRecordId &&
                        (!string.IsNullOrEmpty(scenario) || string.IsNullOrEmpty(p.Scenario)) &&
                        (string.IsNullOrEmpty(scenario) || string.Equals(scenario, p.Scenario, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                var websiteVersionPages = _websiteVersionPages
                    .Where(p =>
                        p.WebsiteVersionId != websiteVersionId ||
                        p.PageId != pageVersion.ParentRecordId ||
                        (string.IsNullOrEmpty(scenario) && !string.IsNullOrEmpty(p.Scenario)) ||
                        (!string.IsNullOrEmpty(scenario) && !string.Equals(scenario, p.Scenario, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                websiteVersionPages.Add(new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = websiteVersionId,
                    PageId = pageVersion.ParentRecordId,
                    PageVersionId = pageVersion.RecordId,
                    Scenario = scenario
                });

                _websiteVersionPages = websiteVersionPages.ToArray();
            }

            foreach (var removed in removedList) {
                AddHistory(
                    new WebsiteVersionRecord { RecordId = removed.WebsiteVersionId },
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "ChildRemoved",
                        ChildType = PageRecord.RecordTypeName,
                        ChildId = removed.PageId,
                        Scenario = scenario
                    });
            }

            AddHistory(
                new WebsiteVersionRecord { RecordId = websiteVersionId },
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "ChildAdded",
                    ChildType = pageVersion.RecordType,
                    ChildId = pageVersion.RecordId,
                    Scenario = scenario
                });
        }

        #endregion

        #region Page versions

        CreateResult IDatabaseUpdater.CreatePageVersion(string identity, PageVersionRecord pageVersion)
        {
            lock (_updateLock)
            {
                pageVersion.RecordId = _pageVersions.OrderByDescending(pv => pv.RecordId).First().RecordId + 1;
                pageVersion.Version = _pageVersions.OrderByDescending(pv => pv.Version).First().Version + 1;
                pageVersion.CreatedWhen = DateTime.UtcNow;
                pageVersion.CreatedBy = identity;

                var pageVersions = _pageVersions.ToList();
                pageVersions.Add(pageVersion);
                _pageVersions = pageVersions.ToArray();

                AddHistory(
                    pageVersion,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                return new CreateResult(pageVersion.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersion(string identity, long pageVersionId, IEnumerable<PropertyChange> changes)
        {
            var pageVersion = _pageVersions.FirstOrDefault(p => p.RecordId == pageVersionId);
            if (pageVersion == null) return new UpdateResult("page_version_not_found", "No page version found with id " + pageVersionId);

            var details = new List<HistoryChangeDetails>();

            foreach (var change in changes)
            {
                var changeDetails = new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = change.PropertyName
                };

                if (change.ArrayIndex.HasValue)
                    changeDetails.FieldName += "[" + change.ArrayIndex.Value + "]";

                if (change.PropertyObject != null)
                    changeDetails.NewValue = JsonConvert.SerializeObject(change.PropertyObject);
                else if (change.PropertyArray != null)
                    changeDetails.NewValue = JsonConvert.SerializeObject(change.PropertyArray);
                else
                    changeDetails.NewValue = JsonConvert.SerializeObject(change.PropertyValue);

                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        changeDetails.OldValue = pageVersion.Name;
                        pageVersion.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        changeDetails.OldValue = pageVersion.DisplayName;
                        pageVersion.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        changeDetails.OldValue = pageVersion.Description;
                        pageVersion.Description = change.PropertyValue;
                        break;
                    case "version":
                        changeDetails.OldValue = pageVersion.Version.ToString();
                        pageVersion.Version = int.Parse(change.PropertyValue);
                        break;
                    case "versionname":
                        changeDetails.OldValue = pageVersion.VersionName;
                        pageVersion.VersionName = change.PropertyValue;
                        break;
                    case "modulename":
                        changeDetails.OldValue = pageVersion.ModuleName;
                        pageVersion.ModuleName = change.PropertyValue;
                        break;
                    case "assetdeployment":
                        changeDetails.OldValue = pageVersion.AssetDeployment.ToString();
                        pageVersion.AssetDeployment = (AssetDeployment)Enum.Parse(typeof(AssetDeployment), change.PropertyValue);
                        break;
                    case "masterpageid":
                        changeDetails.OldValue = pageVersion.MasterPageId.ToString();
                        pageVersion.MasterPageId = string.IsNullOrEmpty(change.PropertyValue) ? (long?)null : long.Parse(change.PropertyValue);
                        break;
                    case "layoutid":
                        changeDetails.OldValue = pageVersion.LayoutId.ToString();
                        pageVersion.LayoutId = string.IsNullOrEmpty(change.PropertyValue) ? (long?)null : long.Parse(change.PropertyValue);
                        break;
                    case "layoutname":
                        changeDetails.OldValue = pageVersion.LayoutName;
                        pageVersion.LayoutName = change.PropertyValue;
                        break;
                    case "canonicalurl":
                        changeDetails.OldValue = pageVersion.CanonicalUrl;
                        pageVersion.CanonicalUrl = change.PropertyValue;
                        break;
                    case "title":
                        changeDetails.OldValue = pageVersion.Title;
                        pageVersion.Title = change.PropertyValue;
                        break;
                    case "bodystyle":
                        changeDetails.OldValue = pageVersion.BodyStyle;
                        pageVersion.BodyStyle = change.PropertyValue;
                        break;
                    case "permission":
                        changeDetails.OldValue = pageVersion.RequiredPermission;
                        pageVersion.RequiredPermission = change.PropertyValue;
                        break;
                    case "assetpath":
                        changeDetails.OldValue = pageVersion.AssetPath;
                        pageVersion.AssetPath = change.PropertyValue;
                        break;
                    case "cachecategory":
                        changeDetails.OldValue = pageVersion.CacheCategory;
                        pageVersion.CacheCategory = change.PropertyValue;
                        break;
                    case "cachepriority":
                        changeDetails.OldValue = pageVersion.CachePriority;
                        pageVersion.CachePriority = change.PropertyValue;
                        break;
                    case "routes":
                        pageVersion.Routes = ArrayPropertyChange(change, changeDetails, pageVersion.Routes, _pageRouteCast);
                        break;
                    case "components":
                        pageVersion.Components = ArrayPropertyChange(change, changeDetails, pageVersion.Components, _elementComponentCast);
                        break;
                    case "layoutzones":
                        pageVersion.LayoutZones = ArrayPropertyChange(change, changeDetails, pageVersion.LayoutZones, _layoutZoneCast);
                        break;
                }
                details.Add(changeDetails);
            }

            AddHistory(pageVersion, identity, details.ToArray());

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersionRoutes(string identity, long pageVersionId, IEnumerable<PageRouteRecord> routes)
        {
            var pageVersion = _pageVersions.FirstOrDefault(p => p.RecordId == pageVersionId);
            if (pageVersion == null) return new UpdateResult("page_version_not_found", "No page version found with id " + pageVersionId);

            var oldRoutes = pageVersion.Routes;
            var newRoutes = routes.ToArray();

            if (oldRoutes != null && oldRoutes.Length == newRoutes.Length)
            {
                var hasChanged = false;
                for (var i = 0; i < oldRoutes.Length; i++)
                {
                    if (oldRoutes[i].Priority != newRoutes[i].Priority) hasChanged = true;
                    if (oldRoutes[i].Path != newRoutes[i].Path) hasChanged = true;
                }
                if (!hasChanged) return new UpdateResult();
            }

            pageVersion.Routes = newRoutes;

            AddHistory(
                pageVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = "routes",
                    OldValue = JsonConvert.SerializeObject(oldRoutes),
                    NewValue = JsonConvert.SerializeObject(newRoutes)
                });

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersionLayoutZones(string identity, long pageVersionId, IEnumerable<LayoutZoneRecord> layoutZones)
        {
            var pageVersion = _pageVersions.FirstOrDefault(p => p.RecordId == pageVersionId);
            if (pageVersion == null) return new UpdateResult("page_version_not_found", "No page version found with id " + pageVersionId);

            var oldZones = pageVersion.LayoutZones;
            var newZones = layoutZones.ToArray();

            if (oldZones != null && oldZones.Length == newZones.Length)
            {
                var hasChanged = false;
                for (var i = 0; i < oldZones.Length; i++)
                {
                    if (oldZones[i].ZoneName != newZones[i].ZoneName) hasChanged = true;
                    if (oldZones[i].ContentName != newZones[i].ContentName) hasChanged = true;
                    if (oldZones[i].ContentType != newZones[i].ContentType) hasChanged = true;
                    if (oldZones[i].ContentValue != newZones[i].ContentValue) hasChanged = true;
                    if (oldZones[i].LayoutId != newZones[i].LayoutId) hasChanged = true;
                    if (oldZones[i].RegionId != newZones[i].RegionId) hasChanged = true;
                }
                if (!hasChanged) return new UpdateResult();
            }

            pageVersion.LayoutZones = newZones;

            AddHistory(
                pageVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = "layoutZones",
                    OldValue = JsonConvert.SerializeObject(oldZones),
                    NewValue = JsonConvert.SerializeObject(newZones)
                });

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersionComponents(string identity, long pageVersionId, IEnumerable<ElementComponentRecord> components)
        {
            var pageVersion = _pageVersions.FirstOrDefault(p => p.RecordId == pageVersionId);
            if (pageVersion == null) return new UpdateResult("page_version_not_found", "No page version found with id " + pageVersionId);

            var oldComponents = pageVersion.Components;
            var newComponents = components.ToArray();

            if (oldComponents != null && oldComponents.Length == newComponents.Length)
            {
                var hasChanged = false;
                for (var i = 0; i < oldComponents.Length; i++)
                {
                    if (oldComponents[i].ComponentName != newComponents[i].ComponentName) hasChanged = true;
                }
                if (!hasChanged) return new UpdateResult();
            }

            pageVersion.Components = newComponents;

            AddHistory(
                pageVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = "components",
                    OldValue = JsonConvert.SerializeObject(oldComponents),
                    NewValue = JsonConvert.SerializeObject(newComponents)
                });

            return new UpdateResult();
        }

        DeleteResult IDatabaseUpdater.DeletePageVersion(string identity, long pageVersionId)
        {
            var pageVersion = _pageVersions.FirstOrDefault(p => p.RecordId == pageVersionId);
            if (pageVersion == null) return new DeleteResult("page_version_not_found", "No page version found with id " + pageVersionId);

            lock (_updateLock)
            {
                _pageVersions = _pageVersions.Where(p => p.RecordId != pageVersionId).ToArray();
            }

            AddHistory(
                pageVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Deleted"
                });

            return new DeleteResult();
        }

        UpdateResult IDatabaseUpdater.RemovePageFromWebsite(string identity, long pageId, long websiteVersionId, string scenario)
        {
            lock (_updateLock)
            {
                _websiteVersionPages =
                    _websiteVersionPages.Where(p =>
                        p.WebsiteVersionId != websiteVersionId ||
                        p.PageId != pageId ||
                        (string.IsNullOrEmpty(scenario) && !string.IsNullOrEmpty(p.Scenario)) ||
                        (!string.IsNullOrEmpty(scenario) && !string.Equals(scenario, p.Scenario, StringComparison.OrdinalIgnoreCase)))
                        .ToArray();
            }

            AddHistory(
                new WebsiteVersionRecord { RecordId = websiteVersionId },
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "ChildRemoved",
                    ChildType = PageRecord.RecordTypeName,
                    ChildId = pageId,
                    Scenario = scenario
                });

            return new UpdateResult();
        }

        #endregion

        #region Layouts

        CreateResult IDatabaseUpdater.CreateLayout(string identity, LayoutRecord layout)
        {
            lock (_updateLock)
            {
                layout.RecordId = _layouts.OrderByDescending(p => p.RecordId).First().RecordId + 1;
                layout.CreatedWhen = DateTime.UtcNow;
                layout.CreatedBy = identity;

                var layouts = _layouts.ToList();
                layouts.Add(layout);
                _layouts = layouts.ToArray();

                AddHistory(
                    layout,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                var layoutVersion = new LayoutVersionRecord
                {
                    RecordId = _layoutVersions.OrderByDescending(pv => pv.RecordId).First().RecordId + 1,
                    CreatedWhen = layout.CreatedWhen,
                    CreatedBy = layout.CreatedBy,
                    ParentRecordId = layout.RecordId,
                    Version = 1
                };

                var layoutVersions = _layoutVersions.ToList();
                layoutVersions.Add(layoutVersion);
                _layoutVersions = layoutVersions.ToArray();

                AddHistory(
                    layoutVersion,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                return new CreateResult(layout.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdateLayout(string identity, long layoutId, IEnumerable<PropertyChange> changes)
        {
            var layout = _layouts.FirstOrDefault(p => p.RecordId == layoutId);
            if (layout == null) return new UpdateResult("layout_not_found", "No layout found with id " + layoutId);

            var details = new List<HistoryChangeDetails>();

            foreach (var change in changes)
            {
                var changeDetails = new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = change.PropertyName,
                    NewValue = change.PropertyValue
                };

                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        changeDetails.OldValue = layout.Name;
                        layout.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        changeDetails.OldValue = layout.DisplayName;
                        layout.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        changeDetails.OldValue = layout.Description;
                        layout.Description = change.PropertyValue;
                        break;
                }
                details.Add(changeDetails);
            }

            AddHistory(layout, identity, details.ToArray());

            return new UpdateResult();
        }

        DeleteResult IDatabaseUpdater.DeleteLayout(string identity, long layoutId)
        {
            var layout = _layouts.FirstOrDefault(p => p.RecordId == layoutId);
            if (layout == null) return new DeleteResult("layout_not_found", "No layout found with id " + layoutId);

            lock (_updateLock)
            {
                _layouts = _layouts.Where(p => p.RecordId != layoutId).ToArray();
            }

            AddHistory(
                layout,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Deleted"
                });

            return new DeleteResult();
        }

        UpdateResult IDatabaseUpdater.AddLayoutToWebsiteVersion(string identity, long layoutId, int version, long websiteVersionId, string scenario)
        {
            var layoutVersion = _layoutVersions.FirstOrDefault(pv => pv.ParentRecordId == layoutId && pv.Version == version);
            if (layoutVersion == null)
            {
                return new UpdateResult(
                    "layout_version_not_found",
                    "There is no version " + version + " of layout id " + layoutId);
            }

            AddLayoutVersionToWebsiteVersion(identity, layoutVersion, websiteVersionId, scenario);

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.AddLayoutToWebsiteVersion(string identity, long layoutVersionId, long websiteVersionId, string scenario)
        {
            var layoutVersion = _layoutVersions.FirstOrDefault(pv => pv.RecordId == layoutVersionId);
            if (layoutVersion == null)
            {
                return new UpdateResult(
                    "layout_version_not_found",
                    "There is layout version " + layoutVersionId);
            }

            AddLayoutVersionToWebsiteVersion(identity, layoutVersion, websiteVersionId, scenario);

            return new UpdateResult();
        }

        private void AddLayoutVersionToWebsiteVersion(string identity, LayoutVersionRecord layoutVersion, long websiteVersionId, string scenario)
        {
            IList<WebsiteVersionLayoutRecord> removedList;

            lock (_updateLock)
            {
                removedList = _websiteVersionLayouts
                    .Where(p =>
                        p.WebsiteVersionId == websiteVersionId &&
                        p.LayoutId == layoutVersion.ParentRecordId &&
                        (!string.IsNullOrEmpty(scenario) || string.IsNullOrEmpty(p.Scenario)) &&
                        (string.IsNullOrEmpty(scenario) || string.Equals(scenario, p.Scenario, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                var websiteVersionLayouts = _websiteVersionLayouts
                    .Where(p =>
                        p.WebsiteVersionId != websiteVersionId ||
                        p.LayoutId != layoutVersion.ParentRecordId ||
                        (string.IsNullOrEmpty(scenario) && !string.IsNullOrEmpty(p.Scenario)) ||
                        (!string.IsNullOrEmpty(scenario) && !string.Equals(scenario, p.Scenario, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                websiteVersionLayouts.Add(new WebsiteVersionLayoutRecord
                {
                    WebsiteVersionId = websiteVersionId,
                    LayoutId = layoutVersion.ParentRecordId,
                    LayoutVersionId = layoutVersion.RecordId,
                    Scenario = scenario
                });

                _websiteVersionLayouts = websiteVersionLayouts.ToArray();
            }

            foreach (var removed in removedList)
            {
                AddHistory(
                    new WebsiteVersionRecord { RecordId = removed.WebsiteVersionId },
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "ChildRemoved",
                        ChildType = LayoutRecord.RecordTypeName,
                        ChildId = removed.LayoutId,
                        Scenario = scenario
                    });
            }

            AddHistory(
                new WebsiteVersionRecord { RecordId = websiteVersionId },
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "ChildAdded",
                    ChildType = layoutVersion.RecordType,
                    ChildId = layoutVersion.RecordId,
                    Scenario = scenario
                });
        }

        #endregion

        #region Layout versions

        CreateResult IDatabaseUpdater.CreateLayoutVersion(string identity, LayoutVersionRecord layoutVersion)
        {
            lock (_updateLock)
            {
                layoutVersion.RecordId = _layoutVersions.OrderByDescending(pv => pv.RecordId).First().RecordId + 1;
                layoutVersion.Version = _layoutVersions.OrderByDescending(pv => pv.Version).First().Version + 1;
                layoutVersion.CreatedWhen = DateTime.UtcNow;
                layoutVersion.CreatedBy = identity;

                var layoutVersions = _layoutVersions.ToList();
                layoutVersions.Add(layoutVersion);
                _layoutVersions = layoutVersions.ToArray();

                AddHistory(
                    layoutVersion,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                return new CreateResult(layoutVersion.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdateLayoutVersion(string identity, long layoutVersionId, IEnumerable<PropertyChange> changes)
        {
            var layoutVersion = _layoutVersions.FirstOrDefault(p => p.RecordId == layoutVersionId);
            if (layoutVersion == null) return new UpdateResult("layout_version_not_found", "No layout version found with id " + layoutVersionId);

            var details = new List<HistoryChangeDetails>();

            foreach (var change in changes)
            {
                var changeDetails = new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = change.PropertyName,
                    NewValue = change.PropertyValue
                };

                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        changeDetails.OldValue = layoutVersion.Name;
                        layoutVersion.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        changeDetails.OldValue = layoutVersion.DisplayName;
                        layoutVersion.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        changeDetails.OldValue = layoutVersion.Description;
                        layoutVersion.Description = change.PropertyValue;
                        break;
                    case "version":
                        changeDetails.OldValue = layoutVersion.Version.ToString();
                        layoutVersion.Version = int.Parse(change.PropertyValue);
                        break;
                    case "versionname":
                        changeDetails.OldValue = layoutVersion.VersionName;
                        layoutVersion.VersionName = change.PropertyValue;
                        break;
                    case "modulename":
                        changeDetails.OldValue = layoutVersion.ModuleName;
                        layoutVersion.ModuleName = change.PropertyValue;
                        break;
                    case "assetdeployment":
                        changeDetails.OldValue = layoutVersion.AssetDeployment.ToString();
                        layoutVersion.AssetDeployment = (AssetDeployment)Enum.Parse(typeof(AssetDeployment), change.PropertyValue);
                        break;
                    case "zonenesting":
                        changeDetails.OldValue = layoutVersion.ZoneNesting;
                        layoutVersion.ZoneNesting = change.PropertyValue;
                        break;
                    case "tag":
                        changeDetails.OldValue = layoutVersion.ContainerTag;
                        layoutVersion.ContainerTag = change.PropertyValue;
                        break;
                    case "style":
                        changeDetails.OldValue = layoutVersion.ContainerStyle;
                        layoutVersion.ContainerStyle = change.PropertyValue;
                        break;
                    case "classes":
                        changeDetails.OldValue = layoutVersion.ContainerClasses;
                        layoutVersion.ContainerClasses = change.PropertyValue;
                        break;
                    case "nestingtag":
                        changeDetails.OldValue = layoutVersion.NestingTag;
                        layoutVersion.NestingTag = change.PropertyValue;
                        break;
                    case "nestingstyle":
                        changeDetails.OldValue = layoutVersion.NestingStyle;
                        layoutVersion.NestingStyle = change.PropertyValue;
                        break;
                    case "nestingclasses":
                        changeDetails.OldValue = layoutVersion.NestingClasses;
                        layoutVersion.NestingClasses = change.PropertyValue;
                        break;
                    case "components":
                        layoutVersion.Components = ArrayPropertyChange(change, changeDetails, layoutVersion.Components, _elementComponentCast);
                        break;
                    case "zones":
                        layoutVersion.Zones = ArrayPropertyChange(change, changeDetails, layoutVersion.Zones, _layoutZoneCast);
                        break;
                }
                details.Add(changeDetails);
            }

            AddHistory(layoutVersion, identity, details.ToArray());

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdateLayoutVersionZones(string identity, long layoutVersionId, IEnumerable<LayoutZoneRecord> layoutZones)
        {
            var layoutVersion = _layoutVersions.FirstOrDefault(p => p.RecordId == layoutVersionId);
            if (layoutVersion == null) return new UpdateResult("layout_version_not_found", "No layout version found with id " + layoutVersionId);

            var oldZones = layoutVersion.Zones;
            var newZones = layoutZones.ToArray();

            if (oldZones != null && oldZones.Length == newZones.Length)
            {
                var hasChanged = false;
                for (var i = 0; i < oldZones.Length; i++)
                {
                    if (oldZones[i].ZoneName != newZones[i].ZoneName) hasChanged = true;
                    if (oldZones[i].ContentName != newZones[i].ContentName) hasChanged = true;
                    if (oldZones[i].ContentType != newZones[i].ContentType) hasChanged = true;
                    if (oldZones[i].ContentValue != newZones[i].ContentValue) hasChanged = true;
                    if (oldZones[i].LayoutId != newZones[i].LayoutId) hasChanged = true;
                    if (oldZones[i].RegionId != newZones[i].RegionId) hasChanged = true;
                }
                if (!hasChanged) return new UpdateResult();
            }

            layoutVersion.Zones = newZones;

            AddHistory(
                layoutVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = "layoutZones",
                    OldValue = JsonConvert.SerializeObject(oldZones),
                    NewValue = JsonConvert.SerializeObject(newZones)
                });

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdateLayoutVersionComponents(string identity, long layoutVersionId, IEnumerable<ElementComponentRecord> components)
        {
            var layoutVersion = _layoutVersions.FirstOrDefault(p => p.RecordId == layoutVersionId);
            if (layoutVersion == null) return new UpdateResult("layout_version_not_found", "No layout version found with id " + layoutVersionId);

            var oldComponents = layoutVersion.Components;
            var newComponents = components.ToArray();

            if (oldComponents != null && oldComponents.Length == newComponents.Length)
            {
                var hasChanged = false;
                for (var i = 0; i < oldComponents.Length; i++)
                {
                    if (oldComponents[i].ComponentName != newComponents[i].ComponentName) hasChanged = true;
                }
                if (!hasChanged) return new UpdateResult();
            }

            layoutVersion.Components = newComponents;

            AddHistory(
                layoutVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = "components",
                    OldValue = JsonConvert.SerializeObject(oldComponents),
                    NewValue = JsonConvert.SerializeObject(newComponents)
                });

            return new UpdateResult();
        }

        DeleteResult IDatabaseUpdater.DeleteLayoutVersion(string identity, long layoutVersionId)
        {
            var layoutVersion = _layoutVersions.FirstOrDefault(p => p.RecordId == layoutVersionId);
            if (layoutVersion == null) return new DeleteResult("layout_version_not_found", "No layout version found with id " + layoutVersionId);

            lock (_updateLock)
            {
                _layoutVersions = _layoutVersions.Where(p => p.RecordId != layoutVersionId).ToArray();
            }

            AddHistory(
                layoutVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Deleted"
                });

            return new DeleteResult();
        }

        UpdateResult IDatabaseUpdater.RemoveLayoutFromWebsite(string identity, long layoutId, long websiteVersionId, string scenario)
        {
            lock (_updateLock)
            {
                _websiteVersionLayouts =
                    _websiteVersionLayouts.Where(p =>
                        p.WebsiteVersionId != websiteVersionId ||
                        p.LayoutId != layoutId ||
                        (string.IsNullOrEmpty(scenario) && !string.IsNullOrEmpty(p.Scenario)) ||
                        (!string.IsNullOrEmpty(scenario) && !string.Equals(scenario, p.Scenario, StringComparison.OrdinalIgnoreCase)))
                        .ToArray();
            }

            AddHistory(
                new WebsiteVersionRecord { RecordId = websiteVersionId },
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "ChildRemoved",
                    ChildType = LayoutRecord.RecordTypeName,
                    ChildId = layoutId,
                    Scenario = scenario
                });

            return new UpdateResult();
        }

        #endregion

        #region Regions

        CreateResult IDatabaseUpdater.CreateRegion(string identity, RegionRecord region)
        {
            lock (_updateLock)
            {
                region.RecordId = _regions.OrderByDescending(p => p.RecordId).First().RecordId + 1;
                region.CreatedWhen = DateTime.UtcNow;
                region.CreatedBy = identity;

                var regions = _regions.ToList();
                regions.Add(region);
                _regions = regions.ToArray();

                AddHistory(
                    region,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                var regionVersion = new RegionVersionRecord
                {
                    RecordId = _regionVersions.OrderByDescending(pv => pv.RecordId).First().RecordId + 1,
                    CreatedWhen = region.CreatedWhen,
                    CreatedBy = region.CreatedBy,
                    ParentRecordId = region.RecordId,
                    Version = 1
                };

                var regionVersions = _regionVersions.ToList();
                regionVersions.Add(regionVersion);
                _regionVersions = regionVersions.ToArray();

                AddHistory(
                    regionVersion,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                return new CreateResult(region.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdateRegion(string identity, long regionId, IEnumerable<PropertyChange> changes)
        {
            var region = _regions.FirstOrDefault(p => p.RecordId == regionId);
            if (region == null) return new UpdateResult("region_not_found", "No region found with id " + regionId);

            var details = new List<HistoryChangeDetails>();

            foreach (var change in changes)
            {
                var changeDetails = new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = change.PropertyName,
                    NewValue = change.PropertyValue
                };

                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        changeDetails.OldValue = region.Name;
                        region.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        changeDetails.OldValue = region.DisplayName;
                        region.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        changeDetails.OldValue = region.Description;
                        region.Description = change.PropertyValue;
                        break;
                }
                details.Add(changeDetails);
            }

            AddHistory(region, identity, details.ToArray());

            return new UpdateResult();
        }

        DeleteResult IDatabaseUpdater.DeleteRegion(string identity, long regionId)
        {
            var region = _regions.FirstOrDefault(p => p.RecordId == regionId);
            if (region == null) return new DeleteResult("region_not_found", "No region found with id " + regionId);

            lock (_updateLock)
            {
                _regions = _regions.Where(p => p.RecordId != regionId).ToArray();
            }

            AddHistory(
                region,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Deleted"
                });

            return new DeleteResult();
        }

        UpdateResult IDatabaseUpdater.AddRegionToWebsiteVersion(string identity, long regionId, int version, long websiteVersionId, string scenario)
        {
            var regionVersion = _regionVersions.FirstOrDefault(pv => pv.ParentRecordId == regionId && pv.Version == version);
            if (regionVersion == null)
            {
                return new UpdateResult(
                    "region_version_not_found",
                    "There is no version " + version + " of region id " + regionId);
            }

            AddRegionVersionToWebsiteVersion(identity, regionVersion, websiteVersionId, scenario);

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.AddRegionToWebsiteVersion(string identity, long regionVersionId, long websiteVersionId, string scenario)
        {
            var regionVersion = _regionVersions.FirstOrDefault(pv => pv.RecordId == regionVersionId);
            if (regionVersion == null)
            {
                return new UpdateResult(
                    "region_version_not_found",
                    "There is region version " + regionVersionId);
            }

            AddRegionVersionToWebsiteVersion(identity, regionVersion, websiteVersionId, scenario);

            return new UpdateResult();
        }

        private void AddRegionVersionToWebsiteVersion(string identity, RegionVersionRecord regionVersion, long websiteVersionId, string scenario)
        {
            IList<WebsiteVersionRegionRecord> removedList;

            lock (_updateLock)
            {
                removedList = _websiteVersionRegions
                    .Where(p =>
                        p.WebsiteVersionId == websiteVersionId &&
                        p.RegionId == regionVersion.ParentRecordId &&
                        (!string.IsNullOrEmpty(scenario) || string.IsNullOrEmpty(p.Scenario)) &&
                        (string.IsNullOrEmpty(scenario) || string.Equals(scenario, p.Scenario, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                var websiteVersionRegions = _websiteVersionRegions
                    .Where(p =>
                        p.WebsiteVersionId != websiteVersionId ||
                        p.RegionId != regionVersion.ParentRecordId ||
                        (string.IsNullOrEmpty(scenario) && !string.IsNullOrEmpty(p.Scenario)) ||
                        (!string.IsNullOrEmpty(scenario) && !string.Equals(scenario, p.Scenario, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                websiteVersionRegions.Add(new WebsiteVersionRegionRecord
                {
                    WebsiteVersionId = websiteVersionId,
                    RegionId = regionVersion.ParentRecordId,
                    RegionVersionId = regionVersion.RecordId,
                    Scenario = scenario
                });

                _websiteVersionRegions = websiteVersionRegions.ToArray();
            }

            foreach (var removed in removedList)
            {
                AddHistory(
                    new WebsiteVersionRecord { RecordId = removed.WebsiteVersionId },
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "ChildRemoved",
                        ChildType = RegionRecord.RecordTypeName,
                        ChildId = removed.RegionId,
                        Scenario = scenario
                    });
            }

            AddHistory(
                new WebsiteVersionRecord { RecordId = websiteVersionId },
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "ChildAdded",
                    ChildType = regionVersion.RecordType,
                    ChildId = regionVersion.RecordId,
                    Scenario = scenario
                });
        }

        #endregion

        #region Region versions

        CreateResult IDatabaseUpdater.CreateRegionVersion(string identity, RegionVersionRecord regionVersion)
        {
            lock (_updateLock)
            {
                regionVersion.RecordId = _regionVersions.OrderByDescending(pv => pv.RecordId).First().RecordId + 1;
                regionVersion.Version = _regionVersions.OrderByDescending(pv => pv.Version).First().Version + 1;
                regionVersion.CreatedWhen = DateTime.UtcNow;
                regionVersion.CreatedBy = identity;

                var regionVersions = _regionVersions.ToList();
                regionVersions.Add(regionVersion);
                _regionVersions = regionVersions.ToArray();

                AddHistory(
                    regionVersion,
                    identity,
                    new HistoryChangeDetails
                    {
                        ChangeType = "Created"
                    });

                return new CreateResult(regionVersion.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdateRegionVersion(string identity, long regionVersionId, IEnumerable<PropertyChange> changes)
        {
            var regionVersion = _regionVersions.FirstOrDefault(p => p.RecordId == regionVersionId);
            if (regionVersion == null) return new UpdateResult("region_version_not_found", "No region version found with id " + regionVersionId);

            var details = new List<HistoryChangeDetails>();

            foreach (var change in changes)
            {
                var changeDetails = new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = change.PropertyName,
                    NewValue = change.PropertyValue
                };

                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        changeDetails.OldValue = regionVersion.Name;
                        regionVersion.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        changeDetails.OldValue = regionVersion.DisplayName;
                        regionVersion.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        changeDetails.OldValue = regionVersion.Description;
                        regionVersion.Description = change.PropertyValue;
                        break;
                    case "version":
                        changeDetails.OldValue = regionVersion.Version.ToString();
                        regionVersion.Version = int.Parse(change.PropertyValue);
                        break;
                    case "versionname":
                        changeDetails.OldValue = regionVersion.VersionName;
                        regionVersion.VersionName = change.PropertyValue;
                        break;
                    case "modulename":
                        changeDetails.OldValue = regionVersion.ModuleName;
                        regionVersion.ModuleName = change.PropertyValue;
                        break;
                    case "assetdeployment":
                        changeDetails.OldValue = regionVersion.AssetDeployment.ToString();
                        regionVersion.AssetDeployment = (AssetDeployment)Enum.Parse(typeof(AssetDeployment), change.PropertyValue);
                        break;
                    case "assetname":
                        changeDetails.OldValue = regionVersion.AssetName.ToString();
                        regionVersion.AssetName = change.PropertyValue;
                        break;
                    case "assetvalue":
                        changeDetails.OldValue = regionVersion.AssetValue.ToString();
                        regionVersion.AssetName = change.PropertyValue;
                        break;
                    case "layoutid":
                        changeDetails.OldValue = regionVersion.LayoutId.ToString();
                        regionVersion.LayoutId = string.IsNullOrEmpty(change.PropertyValue) ? (long?)null : long.Parse(change.PropertyValue);
                        break;
                    case "layoutname":
                        changeDetails.OldValue = regionVersion.LayoutName;
                        regionVersion.LayoutName = change.PropertyValue;
                        break;
                    case "componentid":
                        changeDetails.OldValue = regionVersion.LayoutId.ToString();
                        regionVersion.ComponentId = string.IsNullOrEmpty(change.PropertyValue) ? (long?)null : long.Parse(change.PropertyValue);
                        break;
                    case "componentname":
                        changeDetails.OldValue = regionVersion.ComponentName;
                        regionVersion.ComponentName = change.PropertyValue;
                        break;
                    case "listDatascopeid":
                        changeDetails.OldValue = regionVersion.ListDataScopeId.ToString();
                        regionVersion.ListDataScopeId = string.IsNullOrEmpty(change.PropertyValue) ? (long?)null : long.Parse(change.PropertyValue);
                        break;
                    case "listdatascopename":
                        changeDetails.OldValue = regionVersion.ListDataScopeName;
                        regionVersion.ListDataScopeName = change.PropertyValue;
                        break;
                    case "repeatdatascopeid":
                        changeDetails.OldValue = regionVersion.RepeatDataScopeId.ToString();
                        regionVersion.RepeatDataScopeId = string.IsNullOrEmpty(change.PropertyValue) ? (long?)null : long.Parse(change.PropertyValue);
                        break;
                    case "repeatdatascopename":
                        changeDetails.OldValue = regionVersion.RepeatDataScopeName;
                        regionVersion.RepeatDataScopeName = change.PropertyValue;
                        break;
                    case "listelementclasses":
                        changeDetails.OldValue = regionVersion.ListElementClasses;
                        regionVersion.ListElementClasses = change.PropertyValue;
                        break;
                    case "listelementstyle":
                        changeDetails.OldValue = regionVersion.ListElementStyle;
                        regionVersion.ListElementStyle = change.PropertyValue;
                        break;
                    case "listelementtag":
                        changeDetails.OldValue = regionVersion.ListElementTag;
                        regionVersion.ListElementTag = change.PropertyValue;
                        break;
                    case "components":
                        regionVersion.Components = ArrayPropertyChange(change, changeDetails, regionVersion.Components, _elementComponentCast);
                        break;
                    case "layoutzones":
                        regionVersion.LayoutZones = ArrayPropertyChange(change, changeDetails, regionVersion.LayoutZones, _layoutZoneCast);
                        break;
                    case "regiontemplates":
                        regionVersion.RegionTemplates = ArrayPropertyChange(change, changeDetails, regionVersion.RegionTemplates, _regionTemplateCast);
                        break;
                    case "datascopes":
                        regionVersion.DataScopes = ArrayPropertyChange(change, changeDetails, regionVersion.DataScopes, _elementDataScopeCast);
                        break;
                    case "datatypes":
                        regionVersion.DataTypes = ArrayPropertyChange(change, changeDetails, regionVersion.DataTypes, _elementDataTypeCast);
                        break;
                }
                details.Add(changeDetails);
            }

            AddHistory(regionVersion, identity, details.ToArray());

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdateRegionVersionTemplates(string identity, long regionVersionId, IEnumerable<RegionTemplateRecord> templates)
        {
            var regionVersion = _regionVersions.FirstOrDefault(p => p.RecordId == regionVersionId);
            if (regionVersion == null) return new UpdateResult("region_version_not_found", "No region version found with id " + regionVersionId);

            var oldTemplates = regionVersion.RegionTemplates;
            var newTemplates = templates.ToArray();

            if (oldTemplates != null && oldTemplates.Length == newTemplates.Length)
            {
                var hasChanged = false;
                for (var i = 0; i < oldTemplates.Length; i++)
                {
                    if (oldTemplates[i].TemplatePath != newTemplates[i].TemplatePath) hasChanged = true;
                    if (oldTemplates[i].PageArea != newTemplates[i].PageArea) hasChanged = true;
                }
                if (!hasChanged) return new UpdateResult();
            }

            regionVersion.RegionTemplates = newTemplates;

            AddHistory(
                regionVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = "regionTemplates",
                    OldValue = JsonConvert.SerializeObject(oldTemplates),
                    NewValue = JsonConvert.SerializeObject(newTemplates)
                });

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdateRegionVersionLayoutZones(string identity, long regionVersionId, IEnumerable<LayoutZoneRecord> layoutZones)
        {
            var regionVersion = _regionVersions.FirstOrDefault(p => p.RecordId == regionVersionId);
            if (regionVersion == null) return new UpdateResult("region_version_not_found", "No region version found with id " + regionVersionId);

            var oldZones = regionVersion.LayoutZones;
            var newZones = layoutZones.ToArray();

            if (oldZones != null && oldZones.Length == newZones.Length)
            {
                var hasChanged = false;
                for (var i = 0; i < oldZones.Length; i++)
                {
                    if (oldZones[i].ZoneName != newZones[i].ZoneName) hasChanged = true;
                    if (oldZones[i].ContentName != newZones[i].ContentName) hasChanged = true;
                    if (oldZones[i].ContentType != newZones[i].ContentType) hasChanged = true;
                    if (oldZones[i].ContentValue != newZones[i].ContentValue) hasChanged = true;
                    if (oldZones[i].LayoutId != newZones[i].LayoutId) hasChanged = true;
                    if (oldZones[i].RegionId != newZones[i].RegionId) hasChanged = true;
                }
                if (!hasChanged) return new UpdateResult();
            }

            regionVersion.LayoutZones = newZones;

            AddHistory(
                regionVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = "layoutZones",
                    OldValue = JsonConvert.SerializeObject(oldZones),
                    NewValue = JsonConvert.SerializeObject(newZones)
                });

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdateRegionVersionComponents(string identity, long regionVersionId, IEnumerable<ElementComponentRecord> components)
        {
            var regionVersion = _regionVersions.FirstOrDefault(p => p.RecordId == regionVersionId);
            if (regionVersion == null) return new UpdateResult("region_version_not_found", "No region version found with id " + regionVersionId);

            var oldComponents = regionVersion.Components;
            var newComponents = components.ToArray();

            if (oldComponents != null && oldComponents.Length == newComponents.Length)
            {
                var hasChanged = false;
                for (var i = 0; i < oldComponents.Length; i++)
                {
                    if (oldComponents[i].ComponentName != newComponents[i].ComponentName) hasChanged = true;
                }
                if (!hasChanged) return new UpdateResult();
            }

            regionVersion.Components = newComponents;

            AddHistory(
                regionVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Modified",
                    FieldName = "components",
                    OldValue = JsonConvert.SerializeObject(oldComponents),
                    NewValue = JsonConvert.SerializeObject(newComponents)
                });

            return new UpdateResult();
        }

        DeleteResult IDatabaseUpdater.DeleteRegionVersion(string identity, long regionVersionId)
        {
            var regionVersion = _regionVersions.FirstOrDefault(p => p.RecordId == regionVersionId);
            if (regionVersion == null) return new DeleteResult("region_version_not_found", "No region version found with id " + regionVersionId);

            lock (_updateLock)
            {
                _regionVersions = _regionVersions.Where(p => p.RecordId != regionVersionId).ToArray();
            }

            AddHistory(
                regionVersion,
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "Deleted"
                });

            return new DeleteResult();
        }

        UpdateResult IDatabaseUpdater.RemoveRegionFromWebsite(string identity, long regionId, long websiteVersionId, string scenario)
        {
            lock (_updateLock)
            {
                _websiteVersionRegions =
                    _websiteVersionRegions.Where(p =>
                        p.WebsiteVersionId != websiteVersionId ||
                        p.RegionId != regionId ||
                        (string.IsNullOrEmpty(scenario) && !string.IsNullOrEmpty(p.Scenario)) ||
                        (!string.IsNullOrEmpty(scenario) && !string.Equals(scenario, p.Scenario, StringComparison.OrdinalIgnoreCase)))
                        .ToArray();
            }

            AddHistory(
                new WebsiteVersionRecord { RecordId = websiteVersionId },
                identity,
                new HistoryChangeDetails
                {
                    ChangeType = "ChildRemoved",
                    ChildType = RegionRecord.RecordTypeName,
                    ChildId = regionId,
                    Scenario = scenario
                });

            return new UpdateResult();
        }

        #endregion

        #region protected helper functions

        protected T[] ArrayPropertyChange<T>(PropertyChange change, HistoryChangeDetails changeDetails, T[] array, DynamicCast<T> dynamicCast) where T: class, new()
        {
            if (change.ArrayIndex.HasValue && change.ArrayIndex.Value < array.Length)
            {
                changeDetails.OldValue = JsonConvert.SerializeObject(array[change.ArrayIndex.Value]);
                array[change.ArrayIndex.Value] = dynamicCast.Cast((JObject)change.PropertyObject);
            }
            else if (change.PropertyArray != null)
            {
                changeDetails.OldValue = JsonConvert.SerializeObject(array);
                return dynamicCast.Cast(change.PropertyArray).ToArray();
            }
            return array;
        }

        #endregion

    }
}
