using System;
using System.Reflection;
using Ioc.Modules;
using Microsoft.Owin;
using Ninject;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Pages.Core;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

[assembly: OwinStartup(typeof(Sample3.Startup))]

namespace Sample3
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            #region Configure the Owin pipeline

            var packageLocator = new PackageLocator()
                .ProbeBinFolderAssemblies()
                .Add(Assembly.GetExecutingAssembly());

            var ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));

            var config = ninject.Get<IConfiguration>();
            var pipelineBuilder = ninject.Get<IBuilder>().EnableTracing();

            pipelineBuilder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/sample3/pages");
            app.UseBuilder(pipelineBuilder);

            #endregion

            #region Initialize the Pages middleware

            var fluentBuilder = ninject.Get<IFluentBuilder>();

            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);
            
            fluentBuilder.Register(Assembly.GetExecutingAssembly(), t => ninject.Get(t));

            ninject.Get<INameManager>().Bind();

            #endregion
        }
    }

    /*
     * Page 1 is an example of a website page that uses the stanndard
     * layout but defines custom content
     */

    [IsPage]
    [Route("/", Methods.Get)]
    [RegionComponent("body", "body")]
    internal class Page1 : PageBase { }

    [IsRegion("body")]
    [Container("div", "{ns}_body-region")]
    internal class BodyRegion : ApplicationElement { }

    [IsComponent("body")]
    [RenderHtml("body", "<p>Body</p>")]
    internal class BodyComponent : ApplicationElement { }

    /*
     * PageBase defines the elements that are common to all pages
     */

    [UsesLayout("layout")]
    internal class PageBase { }

    [IsLayout("layout", "header,body,footer")]
    [UsesRegion("header", "header")]
    [UsesRegion("body", "body")]
    [UsesRegion("footer", "footer")]
    [RegionComponent("header", "header")]
    [RegionComponent("footer", "footer")]
    internal class BasePageLayout : ApplicationElement { }

    [IsRegion("header")]
    [Container("div", "{ns}_header-region")]
    internal class HeaderRegion : ApplicationElement { }

    [IsRegion("footer")]
    [Container("div", "{ns}_footer-region")]
    internal class FooterRegion : ApplicationElement { }

    [IsComponent("header")]
    [RenderHtml("header", "<p>Header</p>")]
    internal class HeaderComponent : ApplicationElement { }

    [IsComponent("footer")]
    [RenderHtml("footer", "<p>Footer</p>")]
    internal class FooterComponent : ApplicationElement { }

    /*
     * Defined the Sample3 package
     */

    [IsPackage("sample3", "app")]
    internal class ApplicationPackage { }

    [PartOf("sample3")]
    internal class ApplicationElement { }
}