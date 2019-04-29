using OwinFramework.Pages.CMS.Runtime.Interfaces.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces
{
    public interface IDatabaseUpdater: IDatabaseReader
    {
        UpdateResult UpdatePage(PageRecord page);
    }
}
