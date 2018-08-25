using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Builders;
using OwinFramework.Pages.Html.Configuration;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Html.Runtime;
using IPackage = Ioc.Modules.IPackage;
using OwinFramework.Pages.Core.Interfaces.Collections;

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
                    // The writers have dependencies so it needs to be constructed by IoC
                    new IocRegistration().Init<IHtmlWriterFactory, HtmlWriterFactory>(),
                    new IocRegistration().Init<IHtmlWriter, HtmlWriter>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<ICssWriterFactory, CssWriterFactory>(),
                    new IocRegistration().Init<ICssWriter, CssWriter>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IJavascriptWriterFactory, JavascriptWriterFactory>(),
                    new IocRegistration().Init<IJavascriptWriter, JavascriptWriter>(IocLifetime.MultiInstance),

                    // These wrap the injected dependencies of classes that are designed to
                    // be base classes that applications can inherit from. The IoC dependencies
                    // are wrapped so that we can add new dependencies in the future without
                    // changing the method signature of the constructor
                    new IocRegistration().Init<IModuleDependenciesFactory, ModuleDependenciesFactory>(),
                    new IocRegistration().Init<IModuleDependencies, ModuleDependencies>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IPageDependenciesFactory, PageDependenciesFactory>(),
                    new IocRegistration().Init<IPageDependencies, PageDependencies>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<ILayoutDependenciesFactory, LayoutDependenciesFactory>(),
                    new IocRegistration().Init<ILayoutDependencies, LayoutDependencies>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IRegionDependenciesFactory, RegionDependenciesFactory>(),
                    new IocRegistration().Init<IRegionDependencies, RegionDependencies>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IComponentDependenciesFactory, ComponentDependenciesFactory>(),
                    new IocRegistration().Init<IComponentDependencies, ComponentDependencies>(IocLifetime.MultiInstance),

                    // These are internal implementations that need to be wired up
                    new IocRegistration().Init<IHtmlConfiguration, HtmlConfiguration>(),
                    new IocRegistration().Init<IRenderContextFactory, RenderContextFactory>(),
                    new IocRegistration().Init<IRenderContext, RenderContext>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IHtmlHelper, HtmlHelper>(),

                    // Elements have depedencies that are wrapped in an interface to avoid
                    // breaking application page constructors if new dependencies are added
                    // later
                    new IocRegistration().Init<IModule, Module>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IPage, Page>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<ILayout, LayoutTemplate>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IRegion, RegionTemplate>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IComponent, ComponentTemplate>(IocLifetime.MultiInstance),

                    // These classes implement the rendering framework. They are thread-safe singletons
                    new IocRegistration().Init<IModuleBuilder, ModuleBuilder>(),
                    new IocRegistration().Init<IPageBuilder, PageBuilder>(),
                    new IocRegistration().Init<ILayoutBuilder, LayoutBuilder>(),
                    new IocRegistration().Init<IRegionBuilder, RegionBuilder>(),
                    new IocRegistration().Init<IComponentBuilder, ComponentBuilder>(),

                    // These are the external dependencies
                    new IocRegistration().Init<IDictionaryFactory>(),
                    new IocRegistration().Init<IRequestRouter>(),
                    new IocRegistration().Init<INameManager>(),
                    new IocRegistration().Init<IAssetManager>(),
                    new IocRegistration().Init<IDataCatalog>(),
                    new IocRegistration().Init<IElementConfiguror>(),
                    new IocRegistration().Init<IFluentBuilder>(),
                    new IocRegistration().Init<IDataConsumerFactory>(),
                    new IocRegistration().Init<IDataScopeProviderFactory>(),
                    new IocRegistration().Init<IDataDependencyFactory>(),
                    new IocRegistration().Init<IDataSupplierFactory>(),
                };
            }
        }

    }
}
