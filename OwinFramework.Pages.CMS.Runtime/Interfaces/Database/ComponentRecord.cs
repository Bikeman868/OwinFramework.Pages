namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of component records
    /// </summary>
    public class ComponentRecord: ElementRecordBase
    {
        public ComponentRecord()
        {
            ElementType = "Component";
        }
    }
}
