using System.Collections.Generic;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    public interface IDatabaseUpdater
    {
        #region CRUD

        CreateResult CreateEnvironment(string identity, EnvironmentRecord environment);
        UpdateResult UpdateEnvironment(string identity, long environmentId, IEnumerable<PropertyChange> changes);
        DeleteResult DeleteEnvironment(string identity, long environmentId);

        CreateResult CreateWebsiteVersion(string identity, WebsiteVersionRecord websiteVersion);
        UpdateResult UpdateWebsiteVersion(string identity, long websiteVersionId, IEnumerable<PropertyChange> changes);
        DeleteResult DeleteWebsiteVersion(string identity, long websiteVersionId);

        CreateResult CreatePage(string identity, PageRecord page);
        UpdateResult UpdatePage(string identity, long pageId, IEnumerable<PropertyChange> changes);
        DeleteResult DeletePage(string identity, long pageId);

        CreateResult CreatePageVersion(string identity, PageVersionRecord pageVersion);
        UpdateResult UpdatePageVersion(string identity, long pageVersionId, IEnumerable<PropertyChange> changes);
        UpdateResult UpdatePageVersionRoutes(string identity, long pageVersionId, IEnumerable<PageRouteRecord> routes);
        UpdateResult UpdatePageVersionLayoutZones(string identity, long pageVersionId, IEnumerable<LayoutZoneRecord> layoutZones);
        UpdateResult UpdatePageVersionComponents(string identity, long pageVersionId, IEnumerable<ElementComponentRecord> components);
        DeleteResult DeletePageVersion(string identity, long pageVersionId);

        #endregion

        #region Many-many relationships

        UpdateResult AddPageToWebsiteVersion(string identity, long pageId, int version, long websiteVersionId);
        UpdateResult AddPageToWebsiteVersion(string identity, long pageVersionId, long websiteVersionId);
        UpdateResult RemovePageFromWebsite(string identity, long pageId, long websiteVersionId);

        #endregion
    }
}
