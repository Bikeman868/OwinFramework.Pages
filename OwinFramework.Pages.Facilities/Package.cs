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
                    new IocRegistration().Init<IArrayFactory, ArrayFactory>(),
                    new IocRegistration().Init<IDictionaryFactory, DictionaryFactory>(),
                    new IocRegistration().Init<IMemoryStreamFactory, MemoryStreamFactory>(),
                    new IocRegistration().Init<IQueueFactory, QueueFactory>(),
                    new IocRegistration().Init<IStringBuilderFactory, StringBuilderFactory>(),

                    new IocRegistration().Init<IElementRegistrar, ElementRegistrar>(),
                    new IocRegistration().Init<IPageBuilder, PageBuilder>(),
                    new IocRegistration().Init<IAssetManager, AssetManager>(),
                    new IocRegistration().Init<ITextManager, TextManager>(),
                    new IocRegistration().Init<INameManager, NameManager>(),

                    new IocRegistration().Init<IRequestRouter, RequestRouter>(),
                };
            }
        }

    }
}
