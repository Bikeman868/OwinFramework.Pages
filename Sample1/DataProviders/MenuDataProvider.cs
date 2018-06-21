using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using Sample1.SamplePackages;

namespace Sample1.DataProviders
{
    [IsDataProvider(typeof(IList<MenuPackage.MenuItem>))]
    public class MenuDataProvider: IDataProvider
    {
        public void EstablishContext(IDataContext dataContext)
        {
            IList<MenuPackage.MenuItem> mainMenu = new List<MenuPackage.MenuItem>();

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
            mainMenu.Add(communityMenu);

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
            mainMenu.Add(newsMenu);

            dataContext.Set(mainMenu);
        }
    }

}