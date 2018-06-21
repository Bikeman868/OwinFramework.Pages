using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace Sample1.SamplePackages
{
    /// <summary>
    /// This demonstrates how to write a code only package. This package defines
    /// a layout called 'menu' that will display a list of 'MenuItem' objects
    /// from the data context.
    /// To use this package put the 'Menu' layout into a region and write a
    /// context handler to add a list of MenuPackage.MenuItem objects to the
    /// data context.
    /// Note that adding the [IsPackahe] attribute will make this package
    /// register automatically if you register the assembly that contains it, 
    /// which means that you can not override the namespace.
    /// If you want to override the namespace on a package that has the
    /// [IsPackage] attibute make sure that you register it manually before
    /// registering the assembly that contains it.
    /// </summary>
    [IsPackage("menu")]
    public class MenuPackage : OwinFramework.Pages.Framework.Runtime.Package
    {
        // https://www.w3schools.com/code/tryit.asp?filename=FSITSDF3RKHE
        private readonly IPackageDependenciesFactory _dependencies;

        public MenuPackage(IPackageDependenciesFactory dependencies)
            : base(dependencies)
        {
            _dependencies = dependencies;
        }

        public class MenuItem
        {
            public string Name { get; set; }
            public List<MenuItem> SubMenu { get; set; }
        }

        [IsDataProvider(typeof(IList<MenuItem>))]
        public class MenuDataProvider
        {
            public void EstablishContext(IDataContext dataContext)
            {
                var menuRoot = new MenuItem { SubMenu = new List<MenuItem>() };

                var communityMenu = new MenuItem
                {
                    Name = "Community",
                    SubMenu = new List<MenuItem>
                    {
                        new MenuItem { Name = "Following" },
                        new MenuItem { Name = "Recent posts" },
                        new MenuItem { Name = "Most popular" },
                        new MenuItem { Name = "Trending" }
                    }
                };
                menuRoot.SubMenu.Add(communityMenu);

                var newsMenu = new MenuItem
                {
                    Name = "News",
                    SubMenu = new List<MenuItem>
                    {
                        new MenuItem { Name = "Today" },
                        new MenuItem { Name = "Popular" },
                        new MenuItem { Name = "Trending" }
                    }
                };
                menuRoot.SubMenu.Add(newsMenu);

                dataContext.Set(menuRoot.SubMenu);
            }
        }

        [IsDataProvider(typeof(IList<MenuItem>), "submenu")]
        [NeedsData(typeof(MenuItem))]
        public class SubMenuDataProvider
        {
            public void EstablishContext(IDataContext dataContext)
            {
                var parent = dataContext.Get<MenuItem>();
                dataContext.Set(parent.SubMenu);
            }
        }

        private class MenuItemComponent: Component
        {
            public MenuItemComponent(IComponentDependenciesFactory dependencies) 
                : base(dependencies) { }

            public override IWriteResult WriteHtml(
                IRenderContext renderContext, 
                IDataContext dataContext, 
                bool includeChildren)
            {
                var menuItem = dataContext.Get<MenuItem>();
                renderContext.Html.Write(menuItem.Name);
                return WriteResult.Continue();
            }
        }

        public IPackage Build(IFluentBuilder builder)
        {
            // This component is used to display menu items
            var menuItemComponent = new MenuItemComponent(_dependencies.ComponentDependenciesFactory);

            // This region is a container for the options on the main menu
            var mainMenuItemRegion = builder.Region()
                .Tag("div")
                .Style("display: block; vertical-align: top;")
                .BindTo<MenuItem>()
                .Component(menuItemComponent)
                .Build();

            // This region is a container for the drop down menu items. It
            // renders one menu item component for each menu item in the sub-menu
            var dropDownMenuRegion = builder.Region()
                .Tag("ul")
                .Style("display: none; position: relative;")
                .ForEach<MenuItem>("li")
                .Component(menuItemComponent)
                .Build();

            // This layout defines the main menu option and the sub-menu that
            // drops down wen the main menu option is tapped
            var menuOptionLayout = builder.Layout()
                .Tag("li")
                .Style("display: inline-block; position: relative; vertical-align: top;")
                .RegionNesting("head,submenu")
                .Region("head", mainMenuItemRegion)
                .Region("submenu", dropDownMenuRegion)
                .Build();

            // This region is the whole menu structure with top level menu 
            // options and sub-menus beneath each option
            builder.Region()
                .Name("menu")
                .Tag("ul")
                .Style("display: block; vertical-align: top; white-space: nowrap;")
                .ForEach<MenuItem>()
                .Layout(menuOptionLayout)
                .Build();

            return this;
        }

    }
}
