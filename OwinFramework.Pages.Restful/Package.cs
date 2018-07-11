using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Restful.Runtime;

namespace OwinFramework.Pages.Restful
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: IPackage
    {
        string IPackage.Name { get { return "Owin Framework restful builder"; } }

        IList<IocRegistration> IPackage.IocRegistrations
        {
            get 
            {
                return new List<IocRegistration>
                {
                    // These are the dependencies provided by this assembly
                    new IocRegistration().Init<IServiceDependenciesFactory, ServiceDependenciesFactory>(),

                    // These are the external dependencies
                    new IocRegistration().Init<IRequestRouter>(),
                    new IocRegistration().Init<INameManager>(),
                    new IocRegistration().Init<IDataCatalog>(),
                };
            }
        }

    }
}
