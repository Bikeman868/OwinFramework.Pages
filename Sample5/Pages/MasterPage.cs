using OwinFramework.Pages.Core.Attributes;

/*
* This is the base page for all pages on the website. It provides a standard
* header, navigation menu and footer, but does not specify the page content of the
* 'body' zone of the page layout, which should be specified for each page
*/

namespace Sample5
{
    /// <summary>
    /// All elements that are part of site navigation inherit from this base class
    /// so that they are all deployed in the same module and have the same namespace
    /// </summary>
    [PartOf("application_package")]
    [DeployedAs("navigation_module")]
    internal class NavigationElement { }

    /// <summary>
    /// All elements that are part of site content inherit from this base class
    /// so that they are all deployed in the same module and have the same namespace
    /// </summary>
    [PartOf("application_package")]
    [DeployedAs("content_module")]
    internal class ContentElement { }

    //------------------------------------------------------------------------------------
    // Header - is the same on all pages

    [IsRegion("title_region")]
    [UsesComponent("page_head")]
    [Container(null)]
    internal class TitleRegion : NavigationElement { }

    [IsRegion("login_region")]
    [UsesComponent("login_dialog")]
    [Container(null)]
    internal class LoginRegion : NavigationElement { }

    [IsLayout("header_bar_layout", "hamburger_zone,login_zone,title_zone")]
    [ZoneRegion("hamburger_zone", "menus:mobile_menu")]
    [ZoneRegion("title_zone", "title_region")]
    [ZoneRegion("login_zone", "login_region")]
    [NeedsComponent("menus:menuStyle1")]
    [NeedsComponent("config_service_client")]
    internal class HeaderBarLayout : NavigationElement { }

    [IsRegion("header_bar_region")]
    [Container("div", "{ns}_header_bar_region")]
    [UsesLayout("header_bar_layout")]
    internal class HeaderBarRegion : NavigationElement { }

    [IsLayout("header_layout", "header_bar_zone,menu_zone")]
    [ZoneRegion("header_bar_zone", "header_bar_region")]
    [ZoneRegion("menu_zone", "menus:desktop_menu")]
    [NeedsComponent("menus:menuStyle1")]
    internal class HeaderLayout : NavigationElement { }

    [IsRegion("header_region")]
    [Container("div", "{ns}_header_region")]
    [UsesLayout("header_layout")]
    internal class HeaderRegion : NavigationElement { }

    //------------------------------------------------------------------------------------
    // Footer - is the same design on all pages

    [IsComponent("footer_component")]
    [RenderHtml("footer.standard", "<p class='{ns}_footer'>Copyright 2020 all rights reserved</p>")]
    internal class FooterComponent : NavigationElement { }

    [IsRegion("footer_region")]
    [Container("div", "{ns}_footer_region")]
    [UsesComponent("footer_component")]
    internal class FooterRegion : NavigationElement { }

    //------------------------------------------------------------------------------------
    // Body - the body region has a different layout on each page

    [IsRegion("body_region")]
    [Container("div", "{ns}_body_region")]
    internal class BodyRegion : ContentElement { }

    //------------------------------------------------------------------------------------
    // Base page for all pages on the website

    [IsLayout("master_page_layout", "resources_zone,header_zone,body_zone,footer_zone")]
    [Container("div", "{ns}_page")]
    [ZoneComponent("resources_zone", "resources")]
    [ZoneRegion("header_zone", "header_region")]
    [ZoneRegion("body_zone", "body_region")]
    [ZoneRegion("footer_zone", "footer_region")]
    internal class MasterPageLayout : NavigationElement { }

    [PartOf("application_package")]
    [UsesLayout("master_page_layout")]
    [DeployedAs("content_module")]
    public class MasterPage { }
}