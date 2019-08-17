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

        CreateResult CreateLayout(string identity, LayoutRecord layout);
        UpdateResult UpdateLayout(string identity, long layoutId, IEnumerable<PropertyChange> changes);
        DeleteResult DeleteLayout(string identity, long layoutId);

        CreateResult CreateLayoutVersion(string identity, LayoutVersionRecord layoutVersion);
        UpdateResult UpdateLayoutVersion(string identity, long layoutVersionId, IEnumerable<PropertyChange> changes);
        UpdateResult UpdateLayoutVersionZones(string identity, long layoutVersionId, IEnumerable<LayoutZoneRecord> layoutZones);
        UpdateResult UpdateLayoutVersionComponents(string identity, long layoutVersionId, IEnumerable<ElementComponentRecord> components);
        DeleteResult DeleteLayoutVersion(string identity, long layoutVersionId);

        CreateResult CreateRegion(string identity, RegionRecord region);
        UpdateResult UpdateRegion(string identity, long regionId, IEnumerable<PropertyChange> changes);
        DeleteResult DeleteRegion(string identity, long regionId);

        CreateResult CreateRegionVersion(string identity, RegionVersionRecord regionVersion);
        UpdateResult UpdateRegionVersion(string identity, long regionVersionId, IEnumerable<PropertyChange> changes);
        UpdateResult UpdateRegionVersionComponents(string identity, long regionVersionId, IEnumerable<ElementComponentRecord> components);
        DeleteResult DeleteRegionVersion(string identity, long regionVersionId);

        #endregion

        #region Many-many relationships

        UpdateResult AddPageToWebsiteVersion(string identity, long pageId, int version, long websiteVersionId, string scenario);
        UpdateResult AddPageToWebsiteVersion(string identity, long pageVersionId, long websiteVersionId, string scenario);
        UpdateResult RemovePageFromWebsite(string identity, long pageId, long websiteVersionId, string scenario);

        UpdateResult AddLayoutToWebsiteVersion(string identity, long layoutId, int version, long websiteVersionId, string scenario);
        UpdateResult AddLayoutToWebsiteVersion(string identity, long layoutVersionId, long websiteVersionId, string scenario);
        UpdateResult RemoveLayoutFromWebsite(string identity, long layoutId, long websiteVersionId, string scenario);

        UpdateResult AddRegionToWebsiteVersion(string identity, long regionId, int version, long websiteVersionId, string scenario);
        UpdateResult AddRegionToWebsiteVersion(string identity, long regionVersionId, long websiteVersionId, string scenario);
        UpdateResult RemoveRegionFromWebsite(string identity, long regionId, long websiteVersionId, string scenario);

        #endregion
    }
}
