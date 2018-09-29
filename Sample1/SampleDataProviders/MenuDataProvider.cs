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
    public class MenuDataProvider: DataProvider
    {
        private readonly IList<MenuPackage.MenuItem> _mainMenu;

        public MenuDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies) 
        {
            _mainMenu = new List<MenuPackage.MenuItem>();

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
            _mainMenu.Add(communityMenu);

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
            _mainMenu.Add(newsMenu);
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            dataContext.Set(_mainMenu);
        }
    }

}