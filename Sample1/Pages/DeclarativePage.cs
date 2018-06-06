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
    /// for the module will be deployed in one file
    /// </summary>
    [IsModule("Navigation", AssetDeployment.PerModule)]
    internal class NavigationModule { }

    [IsModule("Content", AssetDeployment.PerModule)]
    internal class ContentModule { }

    /// <summary>
    /// Components are the lowest level in the element heirachy. They define
    /// the html that is rendered when the page is requested. Regions and layouts
    /// also render html, but this is structural (non-visual)
    /// </summary>
    [IsComponent("header.mainMenu")]
    [RenderText("menu.main", "This is where the main menu html goes")]
    internal class MainMenuComponent { }

    [IsComponent("footer.standard")]
    [RenderText("footer.standard", "This is where the html for the page footer goes")]
    internal class StandardFooterComponent { }

    /// <summary>
    /// A region is a container for a single component or layout
    /// The container can use html and css to define its behaviour
    /// </summary>
    [IsRegion("main.header")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [Container("div", "", "header")]
    internal class MainHeaderRegion { }

    [IsRegion("body")]
    [PartOf("Application")]
    [DeployedAs("Content")]
    [Container("div", "", "body")]
    internal class BodyRegion { }

    [IsRegion("main.footer")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [Container("div", "", "footer")]
    internal class MainFooterRegion { }

    [IsRegion("2col.vertical.fixed.left")]
    [PartOf("Application")]
    [DeployedAs("Content")]
    [Container("div", "", "left")]
    internal class LeftRegion { }

    [IsRegion("2col.vertical.fixed.right")]
    [PartOf("Application")]
    [DeployedAs("Content")]
    [Container("div", "", "right")]
    internal class RightRegion { }

    /// <summary>
    /// A layout is an arrangement of regions. Regions can be grouped
    /// inside containers.
    /// </summary>
    [IsLayout("main", "header,body,footer")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [UsesRegion("header", "main.header")]
    [UsesRegion("body", "body")]
    [UsesRegion("footer", "main.footer")]
    internal class MainLayout { }

    /// <summary>
    /// This layout defined the main body of the home page
    /// </summary>
    [IsLayout("homePage", "left.main")]
    [PartOf("Application")]
    [DeployedAs("Navigation")]
    [Container("div", "", "2col.vertical.fixed")]
    [UsesRegion("left", "2col.vertical.fixed.left")]
    [UsesRegion("main", "2col.vertical.fixed.right")]
    internal class HomePageLayout { }

    [IsPage]
    [UsesLayout("main")]
    [Route("/home.html", Methods.Get)]
    [Description("<p>This is an example of how to add a page declatively using attributes</p>")]
    [Option(OptionType.Method, "GET", "<p>Returns the html for the home page</p>")]
    [Option(OptionType.Header, "Accept", "Must contain text/html, which is only available response format")]
    [Example("<a href='/home.html'>/home.html</a>")]
    [RegionComponent("header", "header.mainMenu")]
    [RegionLayout("body", "homePage")]
    [RegionComponent("footer", "footer.standard")]
    internal class HomePage { }
}