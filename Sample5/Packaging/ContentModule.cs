using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;

namespace Sample5.Packaging
{
    /// <summary>
    /// Defines how elements related to website content are deployed
    /// </summary>
    [IsModule("content_module", AssetDeployment.PerWebsite)]
    internal class ContentModule { }
}