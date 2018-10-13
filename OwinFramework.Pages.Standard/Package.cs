using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Builders;
using OwinFramework.Pages.Html.Configuration;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Html.Runtime;
using IPackage = Ioc.Modules.IPackage;

namespace OwinFramework.Pages.Standard
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: IPackage
    {
        string IPackage.Name { get { return "Owin Framework standard packages"; } }

        IList<IocRegistration> IPackage.IocRegistrations
        {
            get 
            {
                return new List<IocRegistration>
                {
                    new IocRegistration().Init<IPackageDependenciesFactory>(),
                };
            }
        }

    }
}
