namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class LayoutRecord: ElementRecordBase
    {
        public LayoutRecord()
        {
            ElementType = "Layout";
        }
    }
}
