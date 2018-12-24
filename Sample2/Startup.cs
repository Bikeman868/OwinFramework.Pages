#define VERSION1

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

[assembly: OwinStartup(typeof(Sample2.Startup))]

namespace Sample2
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            #region Configure Ninject as the IoC container

            // You can use any IoC container you like. In this example I am using Ninject

            var packageLocator = new PackageLocator()
                .ProbeBinFolderAssemblies()
                .Add(Assembly.GetExecutingAssembly());

            var ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));

            #endregion

            #region Build an Owin pipeline with only the Pages middleware in it

            // To keep this example easy to understand I am only adding the Pages middleware
            // to the OWIN pipeline. In your application you will likely want to add other
            // middleware here - static files, identification and authorization etc

            var config = ninject.Get<IConfiguration>();
            var pipelineBuilder = ninject.Get<IBuilder>().EnableTracing(RequestsToTrace.QueryString);

            pipelineBuilder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/sample2/pages");
            app.UseBuilder(pipelineBuilder);

            #endregion

            #region Choose which builders to use

            // The fluent builder has a plug in architecture. You can plug in builders for
            // layouts, regions, components, pages, data providers, services, modules and packages
            // The OwinFramework.Pages.Html.BuildEngine contains builders for layouts, regions, components and pages

            var fluentBuilder = ninject.Get<IFluentBuilder>();
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            
            #endregion

            #region Find all the layouts, regions, components and pages in my website

            // You can register your layouts, regions, pages etc individually, or
            // you can ask the fluent builder to scan assemblies for classes that
            // are decorated with attributes that identify them as these element types.

            // Note that passing a factory method here allows your layouts, regions, pages etc
            // to use constructor injection. If you do not pass a factory method there then
            // these classes must have a default public constructor.

            fluentBuilder.Register(Assembly.GetExecutingAssembly(), t => ninject.Get(t));

            #endregion

            #region Resolve all the name references and bind everything together

            // When the fluent builder registered all of your regions, layouts etc
            // they were all added to the Name Manager in random order. If these
            // elements refer to each other by name then these name references must
            // be resolved after all of the elements are registered

            ninject.Get<INameManager>().Bind();

            #endregion
        }
    }


#if VERSION1
    [IsPage]                                                   // This is a webpage
    [Route("/", Method.Get)]                                   // This page is served for GET requets for the website root
    [PageTitle("Getting started with Owin Framework Pages")]   // Specifies the page title
    [UsesLayout("homePageLayout")]                             // The layout of this page is 'homePageLayout'
    internal class HomePage { }

    [IsLayout("homePageLayout", "region1")]                    // The 'homePageLayout' has 1 region called 'region1'
    [LayoutRegion("region1", "defaultRegion")]                 // Region 1 is implemented by the 'defaultRegion'
    [RegionComponent("region1", "helloWorld")]                 // Region 1 contains the 'helloWorld' component
    internal class HomePageLayout { }

    [IsRegion("defaultRegion")]                                // Defines the 'defaultRegion'
    internal class DefaultRegion { }

    [IsComponent("helloWorld")]                                // A components called 'helloWorld'
    [RenderHtml("hello-world", "Hello, world")]                // Writes out a paragraph of text. The ID 'hello-world' can be used to provide translations into other locales
    internal class HelloWorldComponent { }
#endif

#if VERSION2
    [IsPage]                                                   // This is a webpage
    [Route("/", Method.Get)]                                   // This page is served for GET requets for the website root
    [PageTitle("Getting started with Owin Framework Pages")]   // Specifies the page title
    [UsesLayout("homePageLayout")]                             // The layout of this page is 'homePageLayout'
    internal class HomePage { }

    [IsLayout("homePageLayout", "region1")]                    // The 'homePageLayout' has 1 region called 'region1'
    [LayoutRegion("region1", "defaultRegion")]                 // Region 1 is implemented by the 'defaultRegion'
    internal class HomePageLayout { }

    [IsRegion("defaultRegion")]                                // Defines the 'defaultRegion'
    [RenderHtml("hello-world", "Hello, world")]                // The default region contains static Html
    internal class DefaultRegion { }
#endif

#if VERSION3
    [IsPage]                                                   // This is a webpage
    [Route("/", Method.Get)]                                   // This page is served for GET requets for the website root
    [PageTitle("Getting started with Owin Framework Pages")]   // Specifies the page title
    [UsesLayout("homePageLayout")]                             // The layout of this page is 'homePageLayout'
    internal class HomePage { }

    [IsLayout("homePageLayout", "region1")]                    // The 'homePageLayout' has 1 region called 'region1'
    [RegionHtml("region1", "hello-world", "Hello, world")]     // Region 1 contains static Html
    internal class HomePageLayout { }
#endif

}