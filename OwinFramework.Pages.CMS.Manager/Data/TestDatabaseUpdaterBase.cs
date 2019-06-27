using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.CMS.Runtime.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;

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
                environment.EnvironmentId = _environments.OrderByDescending(e => e.EnvironmentId).First().EnvironmentId + 1;
                environment.CreatedWhen = DateTime.UtcNow;
                environment.CreatedBy = identity;

                var environments = _environments.ToList();
                environments.Add(environment);
                _environments = environments.ToArray();

                return new CreateResult(environment.EnvironmentId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdateEnvironment(string identity, long environmentId, List<PropertyChange> changes)
        {
            var environment = _environments.FirstOrDefault(p => p.EnvironmentId == environmentId);
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
            var environment = _environments.FirstOrDefault(p => p.EnvironmentId == environmentId);
            if (environment == null) return new DeleteResult("environment_not_found", "No environment found with id " + environmentId);

            lock (_updateLock)
            {
                _environments = _environments.Where(p => p.EnvironmentId != environmentId).ToArray();
            }

            return new DeleteResult();
        }

        #endregion

        #region Pages

        CreateResult IDatabaseUpdater.CreatePage(string identity, PageRecord page)
        {
            lock (_updateLock)
            {
                page.ElementId = _pages.OrderByDescending(p => p.ElementId).First().ElementId + 1;
                page.CreatedWhen = DateTime.UtcNow;
                page.CreatedBy = identity;

                var pages = _pages.ToList();
                pages.Add(page);
                _pages = pages.ToArray();

                var pageVersion = new PageVersionRecord
                {
                    ElementVersionId = _pageVersions.OrderByDescending(pv => pv.ElementVersionId).First().ElementVersionId + 1,
                    CreatedWhen = page.CreatedWhen,
                    CreatedBy = page.CreatedBy,
                    ElementId = page.ElementId,
                    Version = 1
                };

                var pageVersions = _pageVersions.ToList();
                pageVersions.Add(pageVersion);
                _pageVersions = pageVersions.ToArray();

                return new CreateResult(page.ElementId);
            }
        }

        UpdateResult IDatabaseUpdater.UpdatePage(string identity, long pageId, List<PropertyChange> changes)
        {
            var page = _pages.FirstOrDefault(p => p.ElementId == pageId);
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
            var page = _pages.FirstOrDefault(p => p.ElementId == pageId);
            if (page == null) return new DeleteResult("page_not_found", "No page found with id " + pageId);

            lock (_updateLock)
            {
                _pages = _pages.Where(p => p.ElementId != pageId).ToArray();
            }

            return new DeleteResult();
        }

        UpdateResult IDatabaseUpdater.AddPageToWebsiteVersion(string identity, long pageId, int version, long websiteVersionId)
        {
            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.ElementId == pageId && pv.Version == version);
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
                    PageVersionId = pageVersion.ElementVersionId
                });

                _websiteVersionPages = websiteVersionPages.ToArray();
            }

            return new UpdateResult();
        }

        UpdateResult IDatabaseUpdater.AddPageToWebsiteVersion(string identity, long pageVersionId, long websiteVersionId)
        {
            var pageVersion = _pageVersions.FirstOrDefault(pv => pv.ElementVersionId == pageVersionId);
            if (pageVersion == null)
            {
                return new UpdateResult(
                    "page_version_not_found", 
                    "There is page version " + pageVersionId);
            }

            lock (_updateLock)
            {
                var websiteVersionPages = _websiteVersionPages
                    .Where(p => p.WebsiteVersionId != websiteVersionId || p.PageId != pageVersion.ElementId)
                    .ToList();

                websiteVersionPages.Add(new WebsiteVersionPageRecord
                {
                    WebsiteVersionId = websiteVersionId,
                    PageId = pageVersion.ElementId,
                    PageVersionId = pageVersion.ElementVersionId
                });

                _websiteVersionPages = websiteVersionPages.ToArray();
            }

            return new UpdateResult();
        }

        #endregion
    }
}
