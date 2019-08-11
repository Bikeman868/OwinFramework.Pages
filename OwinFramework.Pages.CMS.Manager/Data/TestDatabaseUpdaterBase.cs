using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using OwinFramework.Pages.CMS.Runtime.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.CMS.Manager.Data
{
    public class TestDatabaseUpdaterBase: TestDatabaseReaderBase, IDatabaseUpdater
    {
        private readonly object _updateLock = new object();
        private long _nextHistoryEventId;
        private long _nextHistorySummaryId;

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
                    period.Summaries = new[] {summary};
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
            }

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
            lock (_updateLock)
            {
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
                    FieldName = change.PropertyName,
                    NewValue = change.PropertyValue
                };

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
                    case "versionName":
                        changeDetails.OldValue = pageVersion.VersionName;
                        pageVersion.VersionName = change.PropertyValue;
                        break;
                    case "moduleName":
                        changeDetails.OldValue = pageVersion.ModuleName;
                        pageVersion.ModuleName = change.PropertyValue;
                        break;
                    case "assetDeployment":
                        changeDetails.OldValue = pageVersion.AssetDeployment.ToString();
                        pageVersion.AssetDeployment = (AssetDeployment)Enum.Parse(typeof(AssetDeployment), change.PropertyValue);
                        break;
                    case "masterPageId":
                        changeDetails.OldValue = pageVersion.MasterPageId.ToString();
                        pageVersion.MasterPageId = string.IsNullOrEmpty(change.PropertyValue) ? (long?)null : long.Parse(change.PropertyValue);
                        break;
                    case "layoutId":
                        changeDetails.OldValue = pageVersion.LayoutId.ToString();
                        pageVersion.LayoutId = string.IsNullOrEmpty(change.PropertyValue) ? (long?)null : long.Parse(change.PropertyValue);
                        break;
                    case "layoutName":
                        changeDetails.OldValue = pageVersion.LayoutName;
                        pageVersion.LayoutName = change.PropertyValue;
                        break;
                    case "canonicalUrl":
                        changeDetails.OldValue = pageVersion.CanonicalUrl;
                        pageVersion.CanonicalUrl = change.PropertyValue;
                        break;
                    case "title":
                        changeDetails.OldValue = pageVersion.Title;
                        pageVersion.Title = change.PropertyValue;
                        break;
                    case "bodyStyle":
                        changeDetails.OldValue = pageVersion.BodyStyle;
                        pageVersion.BodyStyle = change.PropertyValue;
                        break;
                    case "permission":
                        changeDetails.OldValue = pageVersion.RequiredPermission;
                        pageVersion.RequiredPermission = change.PropertyValue;
                        break;
                    case "assetPath":
                        changeDetails.OldValue = pageVersion.AssetPath;
                        pageVersion.AssetPath = change.PropertyValue;
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
    }
}
