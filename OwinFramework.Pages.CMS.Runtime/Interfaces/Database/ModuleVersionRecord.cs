namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of module version records
    /// </summary>
    public class ModuleVersionRecord: ElementVersionRecordBase
    {
        public const string RecordTypeName = "ModuleVersion";

        public ModuleVersionRecord()
        {
            RecordType = RecordTypeName;
        }

    }
}
