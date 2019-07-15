using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of website version records
    /// </summary>
    public class WebsiteVersionRecord: RecordBase
    {
        public WebsiteVersionRecord()
        {
            RecordType = "WebsiteVersion";
        }
    }
}
