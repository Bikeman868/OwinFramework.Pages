using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Runtime;
using OwinFramework.Pages.Html.Builders;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html
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
                    // The Html writer has dependencies so it needs to be constructed by IoC
                    new IocRegistration().Init<IHtmlWriterFactory, HtmlWriterFactory>(),
                    new IocRegistration().Init<IHtmlWriter, HtmlWriter>(IocLifetime.MultiInstance),

                    // Pages have depedencies that are wrapped in an interface to avoid
                    // breaking application page constructors if new dependencies are added
                    // later
                    new IocRegistration().Init<IPageDependenciesFactory, PageDependenciesFactory>(),
                    new IocRegistration().Init<IRenderContextFactory, RenderContextFactory>(),
                    new IocRegistration().Init<IPageDependencies, PageDependencies>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IRenderContext, RenderContext>(IocLifetime.MultiInstance),

                    // These classes implement the rendering framework. They are thread-safe singletons
                    new IocRegistration().Init<IPageBuilder, PageBuilder>(),
                };
            }
        }

    }
}
