using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Builders;
using OwinFramework.Pages.Facilities.Collections;
using OwinFramework.Pages.Facilities.Managers;
using OwinFramework.Pages.Facilities.Runtime;
using IocRegistration = Ioc.Modules.IocRegistration;

namespace OwinFramework.Pages.Facilities
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
                    // These are singleton factories that pool and reuse collections
                    new IocRegistration().Init<IArrayFactory, ArrayFactory>(),
                    new IocRegistration().Init<IDictionaryFactory, DictionaryFactory>(),
                    new IocRegistration().Init<IMemoryStreamFactory, MemoryStreamFactory>(),
                    new IocRegistration().Init<IQueueFactory, QueueFactory>(),
                    new IocRegistration().Init<IStringBuilderFactory, StringBuilderFactory>(),

                    // Data context is a shared concept, it applies to all response producing mechanisms
                    new IocRegistration().Init<IDataContextFactory, DataContextFactory>(),

                    // These classes implement core facilities. They are thread-safe singletons
                    new IocRegistration().Init<IFluentBuilder, FluentBuilder>(),
                    new IocRegistration().Init<IAssetManager, AssetManager>(),
                    new IocRegistration().Init<ITextManager, TextManager>(),
                    new IocRegistration().Init<INameManager, NameManager>(),

                    // The request router is the top level entry point and the only
                    // interface that the middleware depends on
                    new IocRegistration().Init<IRequestRouter, RequestRouter>(),
                };
            }
        }

    }
}
