using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.CMS.Runtime.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.CMS.Manager.Data
{
    public class TestDatabaseUpdaterBase: TestDatabaseReaderBase, IDatabaseUpdater
    {
        private readonly object _updateLock = new object();

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

                return new CreateResult(environment.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdateEnvironment(string identity, long environmentId, IEnumerable<PropertyChange> changes)
        {
            var environment = _environments.FirstOrDefault(p => p.RecordId == environmentId);
            if (environment == null) return new UpdateResult("environment_not_found", "No environment found with id " + environmentId);

            foreach (var change in changes)
            {
                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        environment.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        environment.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        environment.Description = change.PropertyValue;
                        break;
                    case "baseurl":
                        environment.BaseUrl = change.PropertyValue;
                        break;
                    case "websiteversionid":
                        environment.WebsiteVersionId = long.Parse(change.PropertyValue);
                        break;
                }
            }

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

                return new CreateResult(websiteVersion.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdateWebsiteVersion(string identity, long websiteVersionId, IEnumerable<PropertyChange> changes)
        {
            var websiteVersion = _websiteVersions.FirstOrDefault(p => p.RecordId == websiteVersionId);
            if (websiteVersion == null) return new UpdateResult("website_version_not_found", "No website version found with id " + websiteVersionId);

            foreach (var change in changes)
            {
                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        websiteVersion.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        websiteVersion.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        websiteVersion.Description = change.PropertyValue;
                        break;
                    case "websiteversionid":
                        websiteVersion.RecordId = long.Parse(change.PropertyValue);
                        break;
                }
            }

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

                return new CreateResult(page.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdatePage(string identity, long pageId, IEnumerable<PropertyChange> changes)
        {
            var page = _pages.FirstOrDefault(p => p.RecordId == pageId);
            if (page == null) return new UpdateResult("page_not_found", "No page found with id " + pageId);

            foreach (var change in changes)
            {
                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        page.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        page.DisplayName = change.PropertyValue;
                        break;
                    case "description":
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

            return new DeleteResult();
        }

        UpdateResult IDatabaseUpdater.AddPageToWebsiteVersion(string identity, long pageId, int version, long websiteVersionId)
        {
            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.ParentRecordId == pageId && pv.Version == version);
            if (pageVersion == null)
            {
                return new UpdateResult(
                    "page_version_not_found", 
                    "There is no version " + version + " of page id " + pageId);
            }

            lock (_updateLock)
            {
                var websiteVersionPages = _websiteVersionPages
                    .Where(p => p.WebsiteVersionId != websiteVersionId || p.PageId != pageId)
                    .ToList();

                websiteVersionPages.Add(new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = websiteVersionId,
                    PageId = pageId,
                    PageVersionId = pageVersion.RecordId
                });

                _websiteVersionPages = websiteVersionPages.ToArray();
            }

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.AddPageToWebsiteVersion(string identity, long pageVersionId, long websiteVersionId)
        {
            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.RecordId == pageVersionId);
            if (pageVersion == null)
            {
                return new UpdateResult(
                    "page_version_not_found", 
                    "There is page version " + pageVersionId);
            }

            lock (_updateLock)
            {
                var websiteVersionPages = _websiteVersionPages
                    .Where(p => p.WebsiteVersionId != websiteVersionId || p.PageId != pageVersion.ParentRecordId)
                    .ToList();

                websiteVersionPages.Add(new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = websiteVersionId,
                    PageId = pageVersion.ParentRecordId,
                    PageVersionId = pageVersion.RecordId
                });

                _websiteVersionPages = websiteVersionPages.ToArray();
            }

            return new UpdateResult();
        }

        #endregion

        #region Page versions

        CreateResult IDatabaseUpdater.CreatePageVersion(string identity, PageVersionRecord pageVersion)
        {
            lock (_updateLock)
            {
                pageVersion.RecordId = _pageVersions.OrderByDescending(pv => pv.RecordId).First().RecordId + 1;
                pageVersion.CreatedWhen = DateTime.UtcNow;
                pageVersion.CreatedBy = identity;

                var pageVersions = _pageVersions.ToList();
                pageVersions.Add(pageVersion);
                _pageVersions = pageVersions.ToArray();

                return new CreateResult(pageVersion.RecordId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersion(string identity, long pageVersionId, IEnumerable<PropertyChange> changes)
        {
            var pageVersion = _pageVersions.FirstOrDefault(p => p.RecordId == pageVersionId);
            if (pageVersion == null) return new UpdateResult("page_version_not_found", "No page version found with id " + pageVersionId);

            foreach (var change in changes)
            {
                switch (change.PropertyName.ToLower())
                {
                    case "name":
                        pageVersion.Name = change.PropertyValue;
                        break;
                    case "displayname":
                        pageVersion.DisplayName = change.PropertyValue;
                        break;
                    case "description":
                        pageVersion.Description = change.PropertyValue;
                        break;
                    case "version":
                        pageVersion.Version = int.Parse(change.PropertyValue);
                        break;
                    case "versionName":
                        pageVersion.VersionName = change.PropertyValue;
                        break;
                    case "moduleName":
                        pageVersion.ModuleName = change.PropertyValue;
                        break;
                    case "assetDeployment":
                        pageVersion.AssetDeployment = (AssetDeployment)Enum.Parse(typeof(AssetDeployment), change.PropertyValue);
                        break;
                    case "masterPageId":
                        pageVersion.MasterPageId = string.IsNullOrEmpty(change.PropertyValue) ? (long?)null : long.Parse(change.PropertyValue);
                        break;
                    case "layoutId":
                        pageVersion.LayoutId = string.IsNullOrEmpty(change.PropertyValue) ? (long?)null : long.Parse(change.PropertyValue);
                        break;
                    case "layoutName":
                        pageVersion.LayoutName = change.PropertyValue;
                        break;
                    case "canonicalUrl":
                        pageVersion.CanonicalUrl = change.PropertyValue;
                        break;
                    case "title":
                        pageVersion.Title = change.PropertyValue;
                        break;
                    case "bodyStyle":
                        pageVersion.BodyStyle = change.PropertyValue;
                        break;
                    case "permission":
                        pageVersion.RequiredPermission = change.PropertyValue;
                        break;
                    case "assetPath":
                        pageVersion.AssetPath = change.PropertyValue;
                        break;
                }
            }

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersionRoutes(string identity, long pageVersionId, IEnumerable<PageRouteRecord> routes)
        {
            var pageVersion = _pageVersions.FirstOrDefault(p => p.RecordId == pageVersionId);
            if (pageVersion == null) return new UpdateResult("page_version_not_found", "No page version found with id " + pageVersionId);

            pageVersion.Routes = routes.ToArray();

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersionLayoutZones(string identity, long pageVersionId, IEnumerable<LayoutZoneRecord> layoutZones)
        {
            var pageVersion = _pageVersions.FirstOrDefault(p => p.RecordId == pageVersionId);
            if (pageVersion == null) return new UpdateResult("page_version_not_found", "No page version found with id " + pageVersionId);

            pageVersion.LayoutZones = layoutZones.ToArray();

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.UpdatePageVersionComponents(string identity, long pageVersionId, IEnumerable<ElementComponentRecord> components)
        {
            var pageVersion = _pageVersions.FirstOrDefault(p => p.RecordId == pageVersionId);
            if (pageVersion == null) return new UpdateResult("page_version_not_found", "No page version found with id " + pageVersionId);

            pageVersion.Components = components.ToArray();

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

            return new DeleteResult();
        }

        UpdateResult IDatabaseUpdater.RemovePageFromWebsite(string identity, long pageId, long websiteVersionId)
        {
            lock (_updateLock)
            {
                _websiteVersionPages =
                    _websiteVersionPages.Where(p => p.WebsiteVersionId != websiteVersionId || p.PageId != pageId)
                        .ToArray();
            }

            return new UpdateResult();
        }

        #endregion
    }
}
