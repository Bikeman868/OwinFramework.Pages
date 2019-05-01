namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of page records
    /// </summary>
    public class PageRecord: ElementRecordBase
    {
        public PageRecord()
        {
            ElementType = "Page";
        }
    }
}
