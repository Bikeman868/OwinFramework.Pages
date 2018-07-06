using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataSupplierFactory: IDataSupplierFactory
    {
        private readonly IIdManager _idManager;

        public DataSupplierFactory(
            IIdManager idManager)
        {
            _idManager = idManager;
        }

        public IDataSupplier Create()
        {
            return new DataSupplier(_idManager);
        }
    }
}
