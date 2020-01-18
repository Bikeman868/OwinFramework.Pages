using System.Linq;
using System.Collections.Generic;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
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
        private readonly IList<MenuPackage.MenuItem> _desktopMenu;
        private readonly IList<MenuPackage.MenuItem> _mobileMenu;
        private readonly IList<MenuPackage.MenuItem> _adminMenu;

        public MenuDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies) 
        {
            var menu1 = new MenuPackage.MenuItem
            {
                Name = "Menu 1",
                SubMenu = new[]
                    {
                        new MenuPackage.MenuItem { Name = "Sub menu 1", Url = "/pages/1" },
                        new MenuPackage.MenuItem { Name = "Sub menu 2", Url = "/pages/2" },
                        new MenuPackage.MenuItem { Name = "Sub menu 3", Url = "/pages/3" },
                    }
            };

            var menu2 = new MenuPackage.MenuItem
            {
                Name = "Account",
                SubMenu = new[]
                    {
                        new MenuPackage.MenuItem { Name = "Login", Url = "/account/login" },
                        new MenuPackage.MenuItem { Name = "Register", Url = "/account/register" },
                        new MenuPackage.MenuItem { Name = "Change password", Url = "/account/change-password" },
                        new MenuPackage.MenuItem { Name = "Change email", Url = "/account/change-email" },
                        new MenuPackage.MenuItem { Name = "Reset password", Url = "/account/reset-password" },
                        new MenuPackage.MenuItem { Name = "Verify email", Url = "/account/verify-email-again" },
                    }
            };

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

            _desktopMenu = new List<MenuPackage.MenuItem> 
            { 
                menu1,
                menu2
            };

            _mobileMenu = new List<MenuPackage.MenuItem>
            {
                menu1,
            };

            _adminMenu = new List<MenuPackage.MenuItem>
            {
                menu1,
                menu2,
                adminMenu
            };
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            var authorization = renderContext.OwinContext.GetFeature<IAuthorization>();
            if (authorization != null && authorization.IsInRole("sys.admin"))
            {
                dataContext.Set(_adminMenu, dependency.ScopeName);
                return;
            }

            switch (dependency.ScopeName?.ToLower())
            {
                case "desktop":
                    dataContext.Set(_desktopMenu, dependency.ScopeName);
                    break;
                default:
                    dataContext.Set(_mobileMenu, dependency.ScopeName);
                    break;
            }
        }
    }
}