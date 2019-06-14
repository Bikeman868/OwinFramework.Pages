using System.Collections.Generic;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    public interface IDatabaseUpdater
    {
        CreateResult CreatePage(PageRecord page);
        UpdateResult UpdatePage(long pageId, List<PropertyChange> changes);
        DeleteResult DeletePage(long pageId);
    }
}
