using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;
using OwinFramework.Pages.Standard;

namespace Sample5.DataProviders
{
    /// <summary>
    /// This data provider defines the menu options on all pages of the website
    /// </summary>
    [IsDataProvider("menu", typeof(IList<MenuPackage.MenuItem>))]
    [SuppliesData(typeof(IList<MenuPackage.MenuItem>), "mobile")]
    [SuppliesData(typeof(IList<MenuPackage.MenuItem>), "desktop")]
    public class MenuDataProvider : DataProvider
    {
        private readonly IList<MenuPackage.MenuItem> _menu;

        public MenuDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies) 
        {
            var adminMenu = new MenuPackage.MenuItem
            {
                Name = "Admin",
                SubMenu = new []
                    {
                        new MenuPackage.MenuItem { Name = "CMS", Url = "/admin/cms" },
                        new MenuPackage.MenuItem { Name = "Users", Url = "/admin/users" },
                        new MenuPackage.MenuItem { Name = "Permissions", Url = "/admin/permissions" }
                    }
            };

            _menu = new List<MenuPackage.MenuItem> 
            { 
                adminMenu
            };
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            dataContext.Set(_menu, dependency.ScopeName);
        }
    }
}