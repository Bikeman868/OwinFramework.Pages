using Microsoft.Owin;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class DataProviderDependencies: IDataProviderDependencies
    {
        public IDataProviderDependencies Initialize(IOwinContext context)
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}
