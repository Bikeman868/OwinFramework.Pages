using System.Collections.Generic;
using OwinFramework.Pages.CMS.Runtime.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;

namespace OwinFramework.Pages.CMS.Editor.Data
{
    public class TestDatabaseUpdaterBase: TestDatabaseReaderBase, IDatabaseUpdater
    {
        CreateResult IDatabaseUpdater.CreatePage(PageRecord page)
        {
            return new CreateResult(1);
        }

        UpdateResult IDatabaseUpdater.UpdatePage(long pageId, List<PropertyChange> changes)
        {
            return new UpdateResult();
        }

        DeleteResult IDatabaseUpdater.DeletePage(long pageId)
        {
            return new DeleteResult();
        }
    }
}
