using System;
using System.Collections.Generic;
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
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.DebugMiddleware;
using OwinFramework.Pages.Html.Templates;

namespace Sample3.UseCase3
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
            pipelineBuilder.Register(ninject.Get<DebugInfoMiddleware>()).ConfigureWith(config, "/sample3/debugInfo");
            app.UseBuilder(pipelineBuilder);

            #endregion

            #region Initialize the Pages middleware

            var fluentBuilder = ninject.Get<IFluentBuilder>();
            var nameManager = ninject.Get<INameManager>();
            var requestRouter = ninject.Get<IRequestRouter>();

            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            
            fluentBuilder.Register(Assembly.GetExecutingAssembly(), t => ninject.Get(t));

            var uriLoader = ninject.Get<UriLoader>();
            var markdownTemplateParser = ninject.Get<MarkdownParser>();

            uriLoader.LoadUri(
                new Uri("https://raw.githubusercontent.com/Bikeman868/OwinFramework.Middleware/master/OwinFramework.FormIdentification/readme.md"), 
                markdownTemplateParser, 
                "/url/form-identification");

            nameManager.Bind();

            // This code dynamically adds a route from the root path to the 'home' page from the 'usecase3' package
            // In a regular application these routes would be defined statically as attributes on the pages. This
            // project is unusual because it contains multiple use cases which are essentially all different websites
            // built into one assembly
            var package = nameManager.ResolvePackage("usecase3");
            var page1 = nameManager.ResolvePage("home", package);
            requestRouter.Register(
                page1,
                new FilterAllFilters(
                    new FilterByMethod(Methods.Head, Methods.Get),
                    new FilterByPath("/")),
                    -10);
            #endregion
        }
    }

    [IsPage("home")]
    [Route("/uc3", Methods.Get)]
    [UsesLayout("layout")]
    [RegionTemplate("main", "/url/form-identification")]
    internal class Page1 : ApplicationElement { }

    [IsLayout("layout", "main")]
    [LayoutRegion("main", "div")]
    internal class PageLayout : ApplicationElement { }

    [IsRegion("div")]
    internal class DivRegion : ApplicationElement { }

    [IsPackage("usecase3", "uc3")]
    internal class ApplicationPackage { }

    [PartOf("usecase3")]
    internal class ApplicationElement { }
}