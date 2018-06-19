using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class ModuleDependenciesFactory: IModuleDependenciesFactory
    {
        public IModuleDependencies Create()
        {
            return new ModuleDependencies();
        }
    }
}
