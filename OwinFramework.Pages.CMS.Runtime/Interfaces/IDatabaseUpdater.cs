using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    public interface IDatabaseUpdater: IDatabaseReader
    {
        UpdateResult UpdatePage(PageRecord page);
    }
}
