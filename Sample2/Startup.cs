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
using Sample2;

[assembly: OwinStartup(typeof(Startup))]

namespace Sample2
{
    public class Startup
    {
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

            var nameManager = ninject.Get<INameManager>();
            nameManager.Bind();
        }
    }

    [IsPage]                                                   // This is a webpage
    [Route("/", Methods.Get)]                                  // This page is served for GET requets for the website root
    [PageTitle("Getting started with Owin Framework Pages")]   // Specifies the page title
    [UsesLayout("home-page-layout")]                           // The layout of this page is 'home-page-layout'
    internal class HomePage { }

    [IsLayout("home-page-layout", "region1")]                  // The 'home-page-layout' has 1 region
    [UsesRegion("region1", "default-region")]                  // Region 1 is implemented by the 'default-region'
    [RegionComponent("region1", "hello-world")]                // Region 1 contains the 'hello-world' component
    internal class HomePageLayout { }

    [IsRegion("default-region")]                               // Defines the 'default-region'
    internal class DefaultRegion { }

    [IsComponent("hello-world")]                               // A components called 'hello-world'
    [RenderHtml("hello-world", "<p>Hello, world</p>")]         // Writes out a paragraph of text
    internal class HelloWorldComponent { }
}