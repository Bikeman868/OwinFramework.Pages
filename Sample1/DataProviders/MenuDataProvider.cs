using System;
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
        public string Name { get; set; }
        public IPackage Package { get; set; }
        public ICollection<string> Scopes { get { return _scopes; } }

        private readonly IList<string> _scopes = new List<string>();

        public void AddScope(string scopeName)
        {
            _scopes.Add(scopeName);
        }

        public void EstablishContext(IDataContext dataContext, Type dataType)
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