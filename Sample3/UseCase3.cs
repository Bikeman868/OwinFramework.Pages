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
            
            fluentBuilder.Register(typeof(Page1), t => ninject.Get(t));
            fluentBuilder.Register(typeof(PageLayout), t => ninject.Get(t));
            fluentBuilder.Register(typeof(DivRegion), t => ninject.Get(t));
            fluentBuilder.Register(typeof(ApplicationPackage), t => ninject.Get(t));

            var uriLoader = ninject.Get<UriLoader>();
            var markdownTemplateParser = ninject.Get<MarkdownParser>();

            uriLoader.LoadUri(
                new Uri("https://raw.githubusercontent.com/Bikeman868/OwinFramework.Middleware/master/OwinFramework.FormIdentification/readme.md"), 
                markdownTemplateParser, 
                "/url/form-identification");

            nameManager.Bind();

            #endregion
        }
    }

    [IsPage("home")]
    [Route("/", Method.Get)]
    [UsesLayout("layout")]
    [RegionTemplate("main", "/url/form-identification")]
    internal class Page1 : ApplicationElement { }

    [IsLayout("layout", "main")]
    [LayoutZone("main", "div")]
    internal class PageLayout : ApplicationElement { }

    [IsRegion("div")]
    internal class DivRegion : ApplicationElement { }

    [IsPackage("usecase3", "uc3")]
    internal class ApplicationPackage { }

    [PartOf("usecase3")]
    internal class ApplicationElement { }
}