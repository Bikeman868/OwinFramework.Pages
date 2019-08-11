namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of module records
    /// </summary>
    public class ModuleRecord: ElementRecordBase
    {
        public const string RecordTypeName = "Module";

        public ModuleRecord()
        {
            RecordType = RecordTypeName;
        }
    }
}
