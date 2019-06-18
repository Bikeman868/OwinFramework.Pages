using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.CMS.Manager.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces;

namespace OwinFramework.Pages.CMS.Manager
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: Ioc.Modules.IPackage
    {
        string Ioc.Modules.IPackage.Name { get { return "Owin Framework Pages CMS manager"; } }

        IList<IocRegistration> Ioc.Modules.IPackage.IocRegistrations
        {
            get 
            {
                return new List<IocRegistration>
                {
                    // These are provided by this package
                    new IocRegistration().Init<IDatabaseUpdater, TestDatabaseUpdater>(), 
                    new IocRegistration().Init(container => (ILiveUpdateReceiver)container.Resolve<ILiveUpdateSender>()),
                    new IocRegistration().Init<IDataLayer, DataLayer>(),

                    // These are the external dependencies for this package
                    new IocRegistration().Init<ILiveUpdateReceiver>(),
                };
            }
        }

    }
}
