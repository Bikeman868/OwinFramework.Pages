using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces.Builder;
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
