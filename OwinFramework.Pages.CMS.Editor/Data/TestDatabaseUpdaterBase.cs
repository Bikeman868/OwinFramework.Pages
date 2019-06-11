using OwinFramework.Pages.CMS.Runtime.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;

namespace OwinFramework.Pages.CMS.Editor.Data
{
    public class TestDatabaseUpdaterBase: TestDatabaseReaderBase, IDatabaseUpdater
    {
        public CreateResult CreatePage(PageRecord page)
        {
            return new CreateResult(1);
        }

        public UpdateResult UpdatePage(PageRecord page)
        {
            return new UpdateResult();
        }

        public DeleteResult DeletePage(long pageId)
        {
            return new DeleteResult();
        }
    }
}
