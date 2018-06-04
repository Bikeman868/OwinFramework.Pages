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
    /// <summary>
    /// A package defines a namespace. These are used to import 3rd party libraries
    /// and avoid naming conflicts. You can change the namespace of an imported
    /// package within your application to avoid naming conflicts
    /// </summary>
    [IsPackage("Application", "sample1")]
    internal class ApplicationPackage { }

    /// <summary>
    /// A module is a deployment container. All the JavaScript and css
    /// for the contents of a module will be deployed in one file
    /// </summary>
    [IsModule("Navigation", AssetDeployment.PerModule)]
    internal class NavigationModule { }

    /// <summary>
    /// This will generate a different js and css file for assets generated
    /// by the elements in this module
    /// </summary>
    [IsModule("Content", AssetDeployment.PerModule)]
    internal class ContentModule { }

    /// <summary>
    /// This region is a div with a css claaa name
    /// </summary>
    [IsRegion("main.header")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [Container("<div class='header'>", "</div>")]
    internal class MainHeaderRegion { }

    /// <summary>
    /// This region is a div with a css claaa name
    /// </summary>
    [IsRegion("body")]
    [PartOf("Application")]
    [DeployedAs("Content")]
    [Container("<div class='body'>", "</div>")]
    internal class BodyRegion { }

    /// <summary>
    /// This region is a div with a css claaa name
    /// </summary>
    [IsRegion("main.footer")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [Container("<div class='footer'>", "</div>")]
    internal class MainFooterRegion { }

    /// <summary>
    /// This defines the layout that is used throughout the website
    /// </summary>
    [IsLayout("main")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [HasRegion("header", "main.header")]
    [HasRegion("body", "body")]
    [HasRegion("footer", "main.footer")]
    internal class MainLayout { }

    /// <summary>
    /// This layout defined the main body of the home page
    /// </summary>
    [IsLayout("homePage")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [HasRegion("left", "2col.vertical.fixed.left")]
    [HasRegion("main", "2col.vertical.fixed.right")]
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