using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Builders;
using OwinFramework.Pages.Framework.Managers;
using OwinFramework.Pages.Framework.Runtime;

namespace OwinFramework.Pages.Framework
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: Ioc.Modules.IPackage
    {
        string Ioc.Modules.IPackage.Name { get { return "Owin Framework Pages facilities"; } }

        IList<IocRegistration> Ioc.Modules.IPackage.IocRegistrations
        {
            get 
            {
                return new List<IocRegistration>
                {
                    // These interface mappings are internal to this assembly and
                    // just need to be wired up by IoC
                    new IocRegistration().Init<IFrameworkConfiguration, FrameworkConfiguration>(),

                    // Data context is a shared concept, it applies to all response producing mechanisms
                    new IocRegistration().Init<IDataContextFactory, DataContextFactory>(),

                    // These classes implement core facilities. They are thread-safe singletons
                    new IocRegistration().Init<IFluentBuilder, FluentBuilder>(),
                    new IocRegistration().Init<IAssetManager, AssetManager>(),
                    new IocRegistration().Init<INameManager, NameManager>(),

                    // The request router is the top level entry point and the only
                    // interface that the middleware depends on
                    new IocRegistration().Init<IRequestRouter, RequestRouter>(),
                };
            }
        }

    }
}
