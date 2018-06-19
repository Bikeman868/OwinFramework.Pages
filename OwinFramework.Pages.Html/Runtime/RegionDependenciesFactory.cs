using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class RegionDependenciesFactory: IRegionDependenciesFactory
    {
        public IRegionDependencies Create()
        {
            return new RegionDependencies();
        }
    }
}
