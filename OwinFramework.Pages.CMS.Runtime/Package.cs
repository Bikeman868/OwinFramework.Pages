using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using Prius.Contracts.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Data;
using Urchin.Client.Interfaces;

namespace OwinFramework.Pages.CMS.Runtime
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: Ioc.Modules.IPackage
    {
        string Ioc.Modules.IPackage.Name { get { return "Owin Framework Pages CMS runtime"; } }

        IList<IocRegistration> Ioc.Modules.IPackage.IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    // These are internal classes that need wiring up in IoC
                    new IocRegistration().Init<IDatabaseReader, TestDatabaseReader>(),
                    new IocRegistration().Init<ILiveUpdateSender, Synchronization.InProcessSynchronizer>(), 
                    new IocRegistration().Init(container => (ILiveUpdateReceiver)container.Resolve<ILiveUpdateSender>()),

                    // These are the external dependencies for this package that can be
                    // satisfied by installing additional NuGet packages
                    new IocRegistration().Init<IConfigurationStore>(),
                    new IocRegistration().Init<IContextFactory>(),
                    new IocRegistration().Init<ICommandFactory>(),
                };
            }
        }
    }
}
