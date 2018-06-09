using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Builders;
using OwinFramework.Pages.Facilities.Collections;
using OwinFramework.Pages.Facilities.Managers;
using OwinFramework.Pages.Facilities.Runtime;
using IocRegistration = Ioc.Modules.IocRegistration;
using IPackage = Ioc.Modules.IPackage;

namespace OwinFramework.Pages.Facilities
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: IPackage
    {
        string IPackage.Name { get { return "Owin Framework Pages facilities"; } }

        IList<IocRegistration> IPackage.IocRegistrations
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

                    // Data context is a shared concept, it applies to REST APIs and Html pages
                    new IocRegistration().Init<IDataContextFactory, DataContextFactory>(),
                    new IocRegistration().Init<IDataContext, DataContext>(IocLifetime.MultiInstance),

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
