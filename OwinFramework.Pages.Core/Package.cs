using OwinFramework.Pages.Core.Interfaces.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IocRegistration = Ioc.Modules.IocRegistration;
using IPackage = Ioc.Modules.IPackage;

namespace OwinFramework.Pages.Core
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: IPackage
    {
        string IPackage.Name { get { return "Owin Framework Pages core"; } }

        IList<IocRegistration> IPackage.IocRegistrations
        {
            get 
            {
                return new List<IocRegistration>
                {
                    new IocRegistration().Init<IRequestRouter, RequestRouter>(),
                };
            }
        }

    }
}
