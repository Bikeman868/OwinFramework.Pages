using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    internal class BuiltRegion : Region
    {
        public BuiltRegion(IRegionDependenciesFactory regionDependenciesFactory)
            : base(regionDependenciesFactory)
        {
        }
    }
}
