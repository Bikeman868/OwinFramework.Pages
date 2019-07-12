namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class ComponentVersionRecord: ElementVersionRecordBase
    {
        public ComponentVersionRecord()
        {
            RecordType = "ComponentVersion";
        }
    }
}
