using System;
using System.Collections;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class BuiltRegion : Region
    {

        public BuiltRegion(IRegionDependenciesFactory regionDependenciesFactory)
            : base(regionDependenciesFactory)
        {
        }
    }
}
