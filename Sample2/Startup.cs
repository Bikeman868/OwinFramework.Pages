using System;
using System.IO;
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
using Sample2;

[assembly: OwinStartup(typeof(Startup))]

namespace Sample2
{
    public class Startup
    {
        private static IDisposable _configurationFileSource;

        public void Configuration(IAppBuilder app)
        {
            var packageLocator = new PackageLocator()
                .ProbeBinFolderAssemblies()
                .Add(Assembly.GetExecutingAssembly());

            var ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));

            var config = ninject.Get<IConfiguration>();
            var pipelineBuilder = ninject.Get<IBuilder>().EnableTracing();

            pipelineBuilder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/sample2/pages");
            app.UseBuilder(pipelineBuilder);

            var fluentBuilder = ninject.Get<IFluentBuilder>();
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            fluentBuilder.Register(Assembly.GetExecutingAssembly());
        }
    }

    [IsPage]
    [Route("/", Methods.Get)]
    [PageTitle("Getting started with Owin Framework Pages")]
    [UsesLayout("home-page-layout")]
    internal class HomePage { }

    [IsLayout("home-page-layout", "main")]
    [UsesRegion("main", "default-region")]
    [RegionComponent("main", "hello-world")]
    internal class HomePageLayout { }

    [IsRegion("default-region")]
    internal class DefaultRegion { }

    [IsComponent("hello-world")]
    [RenderHtml("hello-world", "<p>Hello, world</p>")]
    internal class HelloWorldComponent { }
}