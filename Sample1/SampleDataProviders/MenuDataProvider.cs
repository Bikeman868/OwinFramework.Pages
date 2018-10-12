using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;
using Sample1.SamplePackages;

namespace Sample1.SampleDataProviders
{
    [IsDataProvider("menu", typeof(IList<MenuPackage.MenuItem>))]
    [SuppliesData(typeof(IList<MenuPackage.MenuItem>), "mobile")]
    [SuppliesData(typeof(IList<MenuPackage.MenuItem>), "desktop")]
    public class MenuDataProvider : DataProvider
    {
        private readonly IList<MenuPackage.MenuItem> _desktopMenu;
        private readonly IList<MenuPackage.MenuItem> _mobileMenu;

        public MenuDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies) 
        {

            var communityMenu = new MenuPackage.MenuItem
            {
                Name = "Menu 1",
                SubMenu = new List<MenuPackage.MenuItem>
                    {
                        new MenuPackage.MenuItem { Name = "Page 1", Url = "/page1" },
                        new MenuPackage.MenuItem { Name = "Page 2", Url = "/page2" },
                        new MenuPackage.MenuItem { Name = "Page 3", Url = "/page3" },
                        new MenuPackage.MenuItem { Name = "Page 4", Url = "/page4" }
                    }
            };

            var newsMenu = new MenuPackage.MenuItem
            {
                Name = "Menu 2",
                SubMenu = new List<MenuPackage.MenuItem>
                    {
                        new MenuPackage.MenuItem { Name = "Today", Url = "#" },
                        new MenuPackage.MenuItem { Name = "Popular", Url = "#" },
                        new MenuPackage.MenuItem { Name = "Trending", Url = "#" }
                    }
            };

            _desktopMenu = new List<MenuPackage.MenuItem> 
            { 
                communityMenu,
                newsMenu
            };

            _mobileMenu = new List<MenuPackage.MenuItem> 
            { 
                newsMenu
            };
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            if (string.IsNullOrEmpty(dependency.ScopeName))
            {
                dataContext.Set(_desktopMenu);
            }
            else
            {
                if (string.Equals(dependency.ScopeName, "desktop", StringComparison.OrdinalIgnoreCase))
                    dataContext.Set(_desktopMenu, "desktop");
                if (string.Equals(dependency.ScopeName, "mobile", StringComparison.OrdinalIgnoreCase))
                    dataContext.Set(_mobileMenu, "mobile");
            }
        }
    }

}