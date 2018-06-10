using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Collections;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Package]
    public class Package: IPackage
    {
        string IPackage.Name { get { return "Owin Framework Pages core"; } }

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

                    // For this middleware to run it just needs an implementation of IRequestRouter
                    new IocRegistration().Init<IRequestRouter>()
                };
            }
        }
    }
}
