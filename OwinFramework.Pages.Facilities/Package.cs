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

                    // The Html writer has dependencies so it needs to be constructed by IoC
                    new IocRegistration().Init<IHtmlWriterFactory, HtmlWriterFactory>(),
                    new IocRegistration().Init<IHtmlWriter, HtmlWriter>(IocLifetime.MultiInstance),

                    // Pages have depedencies that are wrapped in an interface to avoid
                    // breaking application page constructors if new dependencies are added
                    // later
                    new IocRegistration().Init<IPageDependenciesFactory, PageDependenciesFactory>(),
                    new IocRegistration().Init<IRenderContextFactory, RenderContextFactory>(),
                    new IocRegistration().Init<IDataContextFactory, DataContextFactory>(),
                    new IocRegistration().Init<IPageDependencies, PageDependencies>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IRenderContext, RenderContext>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IDataContext, DataContext>(IocLifetime.MultiInstance),

                    // These classes implement the rendering framework. They are thread-safe singletons
                    new IocRegistration().Init<IElementRegistrar, ElementRegistrar>(),
                    new IocRegistration().Init<IPageBuilder, PageBuilder>(),
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
