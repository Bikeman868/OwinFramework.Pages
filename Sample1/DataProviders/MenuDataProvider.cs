using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;
using Sample1.SamplePackages;

namespace Sample1.DataProviders
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
                Name = "Community",
                SubMenu = new List<MenuPackage.MenuItem>
                    {
                        new MenuPackage.MenuItem { Name = "Following", Url = "#" },
                        new MenuPackage.MenuItem { Name = "Recent posts", Url = "#" },
                        new MenuPackage.MenuItem { Name = "Most popular", Url = "#" },
                        new MenuPackage.MenuItem { Name = "Trending", Url = "#" }
                    }
            };
            _mainMenu.Add(communityMenu);

            var newsMenu = new MenuPackage.MenuItem
            {
                Name = "News",
                SubMenu = new List<MenuPackage.MenuItem>
                    {
                        new MenuPackage.MenuItem { Name = "Today", Url = "#" },
                        new MenuPackage.MenuItem { Name = "Popular", Url = "#" },
                        new MenuPackage.MenuItem { Name = "Trending", Url = "#" }
                    }
            };
            _mainMenu.Add(newsMenu);
        }

        public override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            dataContext.Set(_mainMenu);
        }
    }

}