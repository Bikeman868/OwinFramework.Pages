using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Builders;
using OwinFramework.Pages.Html.Runtime;
using IPackage = Ioc.Modules.IPackage;

namespace OwinFramework.Pages.Html
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: IPackage
    {
        string IPackage.Name { get { return "Owin Framework Html builder"; } }

        IList<IocRegistration> IPackage.IocRegistrations
        {
            get 
            {
                return new List<IocRegistration>
                {
                    // The Html writer has dependencies so it needs to be constructed by IoC
                    new IocRegistration().Init<IHtmlWriterFactory, HtmlWriterFactory>(),
                    new IocRegistration().Init<IHtmlWriter, HtmlWriter>(IocLifetime.MultiInstance),

                    // These are internal implementations that need to be wired up
                    new IocRegistration().Init<IPageDependenciesFactory, PageDependenciesFactory>(),
                    new IocRegistration().Init<IPageDependencies, PageDependencies>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IRenderContextFactory, RenderContextFactory>(),
                    new IocRegistration().Init<IRenderContext, RenderContext>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IHtmlHelper, HtmlHelper>(),

                    // Elements have depedencies that are wrapped in an interface to avoid
                    // breaking application page constructors if new dependencies are added
                    // later
                    new IocRegistration().Init<IModule, Module>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IPage, Page>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<ILayout, Layout>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IRegion, Region>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IComponent, Component>(IocLifetime.MultiInstance),

                    // These classes implement the rendering framework. They are thread-safe singletons
                    new IocRegistration().Init<IModuleBuilder, ModuleBuilder>(),
                    new IocRegistration().Init<IPageBuilder, PageBuilder>(),
                    new IocRegistration().Init<ILayoutBuilder, LayoutBuilder>(),
                    new IocRegistration().Init<IRegionBuilder, RegionBuilder>(),
                    new IocRegistration().Init<IComponentBuilder, ComponentBuilder>(),

                    // These are the external dependencies
                    new IocRegistration().Init<IRequestRouter>(),
                    new IocRegistration().Init<INameManager>(),
                };
            }
        }

    }
}
