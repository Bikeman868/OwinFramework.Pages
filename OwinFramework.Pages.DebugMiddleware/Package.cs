using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.DebugMiddleware
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Package]
    public class Package: IPackage
    {
        string IPackage.Name { get { return "Owin Framework Pages debug middleware"; } }

        IList<IocRegistration> IPackage.IocRegistrations
        {
            get 
            {
                return new List<IocRegistration>
                {
                    new IocRegistration().Init<IRequestRouter>(),
                    new IocRegistration().Init<IHtmlWriterFactory>(),
                };
            }
        }
    }
}
