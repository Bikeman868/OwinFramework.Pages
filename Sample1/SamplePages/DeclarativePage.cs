using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using Sample1.SampleDataProviders;

/* 
 * This page demonstrates how to define page elements declaratively. Notice how the
 * classes do no have to have any implementation, have no base class and do not
 * implement any interfaces. The page can entirely defined using attributes only.
 * 
 * We do NOT recommend building your whole website this way, it is not very maintainable,
 * but this is a good technique to use for adding a few very simple elements
 * where writing a class and overriding methods is overkill.
 * 
 * This technique is very useful for defining modules and layouts, and maybe regions
 * if they are simple and not too many. Note that you can always change your mind later
 * and re-define your elements using a class or template without changing any of the
 * places where they are used.
 */

namespace Sample1.SamplePages
{
    /*
     * A package defines a namespace. These are used to import 3rd party libraries
     * and avoid naming conflicts. You can change the namespace of an imported
     * package within your application
     */

    /// <summary>
    /// Defines a package called 'Application' in which all the JavaScript methods
    /// and css classes will be in the 'sample1' namespace
    /// </summary>
    [IsPackage("application", "sample1")]
    internal class ApplicationPackage { }

    /*
     * A module is a deployment container. All the JavaScript and css
     * for the module will be combined in one file. By default you will end 
     * up with one JavaScript and one css file per module, but you can change
     * this for each module, and you can override this behaviour on each
     * element.
     */

    /// <summary>
    /// Defines a module that contains all of the scripts and styles for 
    /// elements that are related to the site navigation. These should be
    /// deployed to the module asset files by default
    /// </summary>
    [IsModule("navigation", AssetDeployment.PerModule)]
    internal class NavigationModule { }

    /// <summary>
    /// Defines a module that contains all of the scripts and styles for 
    /// elements that are related to the site content. These should be
    /// written into the head section of each page
    /// </summary>
    [IsModule("content", AssetDeployment.InPage)]
    internal class ContentModule { }

    /*
     * Components are the lowest level in the element heirachy. They define
     * the html that is rendered when the page is requested. Regions and layouts
     * also render html, but this is structural (non-visual)
     */

    [IsComponent("defaultStyles")]
    [PartOf("application")]
    [DeployedAs("content")]
    [DeployCss("p", "font-size:11pt;")]
    [DeployCss("h1", "font-size:16pt;")]
    [DeployCss("h2", "font-size:14pt;")]
    [DeployCss("h3", "font-size:12pt;")]
    internal class DefaultStylesComponent { }

    [IsComponent("mainMenu")]
    [PartOf("application")]
    [DeployedAs("navigation")]
    [DeployCss("p.{ns}_menu-item", "font-weight:bold;")]
    [RenderHtml("menu.main", "<p class='{ns}_menu-item'>This is where the main menu html goes</p>")]
    internal class MainMenuComponent { }

    [IsComponent("footer")]
    [PartOf("application")]
    [DeployedAs("navigation")]
    [DeployCss("p.{ns}_footer", "font-weight:bold; font-size:9pt;")]
    [DeployFunction("int", "footerFunction", "action", "alert(action);")]
    [RenderHtml("footer.standard", "<p class='{ns}_footer'>This is where the html for the page footer goes</p>")]
    internal class StandardFooterComponent { }

    [IsComponent("sidebar1")]
    [PartOf("application")]
    [DeployedAs("content")]
    [DeployCss("p.{ns}_sidebar", "font-family: arial; font-size:11pt;")]
    [DeployFunction(null, "toggleSideBar", null, "alert('Hello');")]
    [RenderHtml("menu.left", "<p class='{ns}_sidebar'>Side bar nav for page 1</p>")]
    internal class SideBar1Component { }

    [IsComponent("page1Body")]
    [PartOf("application")]
    [DeployedAs("content")]
    [DeployCss("p.{ns}_body", "font-family: arial; font-size:9pt;")]
    [RenderHtml("content.body.1.1", 1, "<p class='{ns}_body'>Hello, page 1</p>")]
    [RenderHtml("content.body.1.2", 2, "<p class='{ns}_body'>This is the second parapgraph</p>")]
    [RenderHtml("content.body.1.3", 3, "<p class='{ns}_body'>This is the third parapgraph</p>")]
    internal class PageBody1Component { }

    [IsComponent("sidebar2")]
    [PartOf("application")]
    [DeployedAs("content")]
    [DeployCss("p.{ns}_sidebar", "font-family: arial; font-size:11pt;")]
    [RenderHtml("menu.left", "<p class='{ns}_sidebar'>Side bar nav for page 2</p>")]
    internal class SideBar2Component { }

    /*
     * A region is a container for a single component or layout. The container can 
     * use html, JavaScript and css to define its behaviour, for example the size, 
     * scroll bars, content overflow, visibiliy and position on the page.
     */

    /// <summary>
    /// Defines a region called 'header' that is at the top of every page
    /// </summary>
    [IsRegion("header")]
    [PartOf("application")]
    [DeployedAs("navigation")]
    [Style("height: 90px; width:100%; padding:10px; background: gray; color: whitesmoke; clear: both;")]
    [UsesLayout("header")]
    internal class MainHeaderRegion { }

    /// <summary>
    /// Defines a region called 'header' that is at the top of every page. It also
    /// writes some initialization JavaScript into the bottom of every page
    /// </summary>
    [IsRegion("title")]
    [PartOf("application")]
    [DeployedAs("navigation")]
    [RenderTemplate("/common/pageTitle", PageArea.Body)]
    [RenderTemplate("/common/pageInitialization", PageArea.Initialization)]
    internal class TitleRegion { }

    /// <summary>
    /// Defines a region called 'body' that holds the main content area of each page
    /// </summary>
    [IsRegion("body")]
    [PartOf("application")]
    [DeployedAs("content")]
    [Style("background: whitesmoke; clear: both; overflow: hidden; white-space: nowrap;")]
    internal class BodyRegion { }

    /// <summary>
    /// Defines a region called 'main.footer' which appears at the bottom of each page
    /// </summary>
    [IsRegion("footer")]
    [PartOf("application")]
    [DeployedAs("navigation")]
    [Style("height: 50px; width:100%; padding:5px; background: gray; color: whitesmoke;")]
    internal class MainFooterRegion { }

    /// <summary>
    /// Defines a region called '2col.vertical.fixed.left' that provides the left
    /// hand column of a 2 vertical column layout
    /// </summary>
    [IsRegion("leftColumn")]
    [PartOf("application")]
    [DeployedAs("content")]
    [Style("width:175px; background: alliceblue; display: inline-block; vertical-align: top; white-space: normal;")]
    internal class LeftRegion { }

    /// <summary>
    /// Defines a region called '2col.vertical.fixed.right' that provides the right
    /// hand column of a 2 vertical column layout
    /// </summary>
    [IsRegion("rightColumn")]
    [PartOf("application")]
    [DeployedAs("content")]
    [Style("display: inline-block; overflow: visible; white-space: normal; vertical-align: top;")]
    internal class RightRegion { }

    /* <summary>
     * A layout is an arrangement of regions. Regions can be grouped
     * inside containers. For example a layout could define a header bar
     * main body and footer bar.
     * Each page has a layout that defines how the page content is arranged.
     * You can also place a layout inside of a region to create a nested
     * structure of layouts within layouts.
     * The layout can be configured to contain certain content by defult
     * in each region but this can be overriden in each place where the
     * layout is used. For example you can use the exact same layout for
     * every page on your website where the contents of the header and
     * footer region are the default on every page but the main region
     * is overriden on each page to make every page unique, but every page
     * have the same header and footer.
     */

    /// <summary>
    /// Defines a layout called 'main' that has three regions. The regions
    /// are rendered in the order 'header', 'body' then 'footer'. By default
    /// the 'header' region contains the 'main.header' region, the 'body' region
    /// contains the 'body' region and the 'footer' region contains 'main.footer'.
    /// The contents of each region can be overriden for each instance of this layout
    /// </summary>
    [IsLayout("main", "header(body,footer)")]
    [PartOf("application")]
    [DeployedAs("navigation")]
    [ZoneRegion("header", "header")]
    [ZoneRegion("body", "body")]
    [ZoneRegion("footer", "footer")]
    [ZoneLayout("header", "header")]
    [ZoneComponent("footer", "footer")]
    internal class MainLayout { }

    /// <summary>
    /// Defines the layout of the 'header' region
    /// </summary>
    [IsLayout("header", "title,menu")]
    [PartOf("application")]
    [DeployedAs("navigation")]
    [ZoneRegion("title", "title")]
    [ZoneRegion("menu", "menus:desktop_menu")]
    internal class HeaderLayout { }

    /// <summary>
    /// Defines the layout of the 'body' region for page 1
    /// </summary>
    [IsLayout("page1Body", "left,main")]
    [PartOf("application")]
    [DeployedAs("content")]
    [Container("div", "2col.vertical.fixed")]
    [ZoneRegion("left", "leftColumn")]
    [ZoneRegion("main", "rightColumn")]
    [ZoneComponent("left", "sidebar1")]
    [ZoneComponent("main", "page1Body")]
    [NeedsComponent("libraries:Redux")]
    internal class Page1Layout { }

    /// <summary>
    /// Defines the layout of the 'body' region for page 2
    /// </summary>
    [IsLayout("page2Body", "left,main")]
    [PartOf("application")]
    [DeployedAs("content")]
    [Container("div", "2col.vertical.fixed")]
    [ZoneRegion("left", "leftColumn")]
    [ZoneRegion("main", "rightColumn")]
    [ZoneComponent("left", "sidebar2")]
    //[ZoneTemplate("main", "/page2/body")]
    //[ZoneTemplate("main", "/file/template1")]
    //[ZoneTemplate("main", "/file/template2")]
    //[ZoneTemplate("main", "/file/template3")]
    [ZoneTemplate("main", "/file/template4")]
    //[ZoneTemplate("main", "/file/template5")]
    //[ZoneTemplate("main", "/file/template6")]
    [NeedsComponent("libraries:Vue")]
    internal class Page2Layout { }

    /*
     * Pages are rendered in response to requests. In order to see your pages
     * you have to define a route for them. Routes can include wildcards and
     * you can attach multiple [Route()] attrinutes.
     * Every page needs a layout, and usually at least one region of the layout 
     * is overriden to make this page unique.
     * You can create self-documentation for your website by adding attributes 
     * like [Description()], [Option()], and [Example()]
     */

    /// <summary>
    /// In this example I made the pages inherit from this base page so that
    /// I can define some attributes that apply to all of the pages.
    /// </summary>
    [PartOf("application")]
    [NeedsComponent("defaultStyles")]
    [NeedsComponent("menus:menuStyle1")]
    [UsesLayout("main")]
    internal class PageBase { }

    /// <summary>
    /// Defines a page that is rendered in response to requets for '/home.html'
    /// Uses the 'main' layout but changes the contents of the 'body' region.
    /// </summary>
    [IsPage("page1")]
    [Description("<p>This is an example of how to add a page declatively using attributes</p>")]
    [Option(OptionType.Method, "GET", "<p>Returns the html for the page 1</p>")]
    [Option(OptionType.Header, "Accept", "Must contain text/html, which is only available response format")]
    [Example("<a href='/home.html'>/home.html</a>")]
    [Example("<a href='/page1'>/page1</a>")]
    [Route("/home.html", Method.Get)]
    [Route("/page1", Method.Get)]
    [PageTitle("Page 1")]
    [Style("color: darkred;")]
    [ZoneLayout("body", "page1Body")]
    [NeedsComponent("libraries:jQuery")]
    [NeedsComponent("libraries:React")]
    [NeedsComponent("libraries:AngularJS")]
    [NeedsComponent("libraries:Redux")]
    internal class Page1 : PageBase { }

    /// <summary>
    /// Defines a page that is rendered in response to requets for '/page2'
    /// Uses the 'main' layout but changes the contents of the 'body' region.
    /// </summary>
    [IsPage("page2")]
    [Description("<p>This is an example of how to add a page declatively using attributes</p>")]
    [Option(OptionType.Method, "GET", "<p>Returns the html for page 2</p>")]
    [Example("<a href='/page2'>/page2</a>")]
    [Route("/page2", Method.Get)]
    [PageTitle("Page 2")]
    [Style("color: darkblue;")]
    [ZoneLayout("body", "page2Body")]
    internal class Page2 : PageBase { }

    /// <summary>
    /// Defines a page that is rendered in response to requets for '/page3'
    /// Uses the 'main' layout but changes the contents of the 'body' region.
    /// </summary>
    [IsPage("page3")]
    [Route("/page3", Method.Get)]
    [PageTitle("Page 3")]
    [ZoneComponent("body", "math_form")]
    internal class Page3 : PageBase { }
}