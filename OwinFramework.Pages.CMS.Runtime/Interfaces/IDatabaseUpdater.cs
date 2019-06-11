using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    public interface IDatabaseUpdater
    {
        CreateResult CreatePage(PageRecord page);
        UpdateResult UpdatePage(PageRecord page);
        DeleteResult DeletePage(long pageId);
    }
}
