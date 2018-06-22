using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Managers
{
    internal class DataCatalog: IDataCatalog
    {
        public IDataCatalog Register(IDataProvider dataProvider)
        {
            return this;
        }

        public T Ensure<T>(IDataContext dataContext) where T : class
        {
            return null;
        }
    }
}
