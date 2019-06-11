using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.CMS.Editor.Data;
using OwinFramework.Pages.CMS.Runtime.Data;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.CMS.Editor
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: Ioc.Modules.IPackage
    {
        string Ioc.Modules.IPackage.Name { get { return "Owin Framework Pages CMS editor"; } }

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
