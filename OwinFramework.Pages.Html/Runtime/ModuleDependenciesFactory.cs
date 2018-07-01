using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class ModuleDependenciesFactory: IModuleDependenciesFactory
    {
        public IModuleDependencies Create(IOwinContext context)
        {
            return new ModuleDependencies();
        }
    }
}
