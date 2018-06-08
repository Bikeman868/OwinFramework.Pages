using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Runtime
{
    internal class DataContextFactory : IDataContextFactory
    {
        IDataContext IDataContextFactory.Create()
        {
            return new DataContext();
        }
    }
}
