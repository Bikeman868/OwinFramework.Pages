using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;

/* 
 * This page demonstrates how to define page elements declaratively. Notice how the
 * classes do no have to have any implementation, have no base class and do not
 * implement any interfaces. The page can entirely defined using attributes only.
 */

namespace Sample1.Pages
{
    [IsModule("Navigation", "site.nav")]
    [DeployedAs(AssetDeployment.InPage)]
    internal class NavigationModule { }

    [IsModule("Content", "site.content")]
    [DeployedAs(AssetDeployment.PerModule)]
    internal class ContentModule { }

    [IsRegion("main.header")]
    [PartOf("Navigation")]
    [DeployedAs(AssetDeployment.InPage)]
    [Container("<div class='header'>", "</div>")]
    internal class MainHeaderRegion { }

    [IsRegion("body")]
    [PartOf("Content")]
    [Container("<div class='body'>", "</div>")]
    internal class BodyRegion { }

    [IsRegion("main.footer")]
    [PartOf("Navigation")]
    [DeployedAs(AssetDeployment.InPage)]
    [Container("<div class='footer'>", "</div>")]
    internal class MainFooterRegion { }

    [IsLayout("main")]
    [HasRegion("header", "main.header")]
    [HasRegion("body", "body")]
    [HasRegion("footer", "main.footer")]
    internal class MainLayout { }

    [IsLayout("homePage")]
    internal class HomePageLayout { }

    [IsPage]
    [HasLayout("main")]
    [Route("/home.html", Methods.Get)]
    [Description("<p>This is an example of how to add a page declatively using attributes</p>")]
    [Option(OptionType.Method, "GET", "<p>Returns the html for the home page</p>")]
    [Option(OptionType.Header, "Accept", "text/html")]
    [Example("<a href='/home.html'>/home.html</a>")]
    [RegionComponent("header", "header.mainMenu")]
    [RegionLayout("body", "homePage")]
    [RegionComponent("footer", "footer.standard")]
    internal class HomePage { }
}