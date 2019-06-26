using System.Collections.Generic;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    public interface IDatabaseUpdater
    {
        CreateResult CreatePage(string identity, PageRecord page);
        UpdateResult UpdatePage(string identity, long pageId, List<PropertyChange> changes);
        DeleteResult DeletePage(string identity, long pageId);

        UpdateResult AddPageToWebsiteVersion(string identity, long pageId, int version, long websiteVersionId);
        UpdateResult AddPageToWebsiteVersion(string identity, long pageVersionId, long websiteVersionId);
    }
}
