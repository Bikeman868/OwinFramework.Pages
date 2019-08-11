namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of layout records
    /// </summary>
    public class ComponentVersionRecord: ElementVersionRecordBase
    {
        public const string RecordTypeName = "ComponentVersion";

        public ComponentVersionRecord()
        {
            RecordType = RecordTypeName;
        }
    }
}
