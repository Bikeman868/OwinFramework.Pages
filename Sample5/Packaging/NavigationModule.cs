using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;

namespace Sample5.Packaging
{
    /// <summary>
    /// Defines how elements related to website navigation are deployed
    /// </summary>
    [IsModule("navigation_module", AssetDeployment.PerWebsite)]
    internal class NavigationModule { }

}