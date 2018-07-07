using System;
using System.IO;
using System.Reflection;
using Ioc.Modules;
using Microsoft.Owin;
using Ninject;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Utility;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.DebugMiddleware;
using Sample1.SamplePackages;
using Sample1.SamplePages;
using Urchin.Client.Sources;
using OwinFramework.Pages.Core;
using Sample1;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

[assembly: OwinStartup(typeof(Startup))]

namespace Sample1
{
    public class Startup
    {
        private static IDisposable _configurationFileSource;

        public void Configuration(IAppBuilder app)
        {
            // The package locator finds assemblies that advertise their IoC requirements
            var packageLocator = new PackageLocator()
                .ProbeBinFolderAssemblies()
                .Add(Assembly.GetExecutingAssembly());

            // For this example we will use Ninject as the IoC container
            var ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));

            // Next we load up the configuration file and watch for changes
            var hostingEnvironment = ninject.Get<IHostingEnvironment>();
            var configFile = new FileInfo(hostingEnvironment.MapPath("config.json"));
            _configurationFileSource = ninject.Get<FileSource>().Initialize(configFile, TimeSpan.FromSeconds(5));

            // Get a reference to the loaded configuration
            var config = ninject.Get<IConfiguration>();

            // Get the Owin Framework pipeline builder
            var pipelineBuilder = ninject.Get<IBuilder>().EnableTracing();

            // Define the middleware to add to the Owin Pipeline
            pipelineBuilder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/sample1/pages");
            pipelineBuilder.Register(ninject.Get<DebugInfoMiddleware>()).ConfigureWith(config, "/sample1/pages/debugInfo");
            pipelineBuilder.Register(ninject.Get<OwinFramework.NotFound.NotFoundMiddleware>()).ConfigureWith(config, "/sample1/notFound");
            pipelineBuilder.Register(ninject.Get<OwinFramework.Documenter.DocumenterMiddleware>()).ConfigureWith(config, "/sample1/documenter").RunFirst();
            pipelineBuilder.Register(ninject.Get<OwinFramework.DefaultDocument.DefaultDocumentMiddleware>()).ConfigureWith(config, "/sample1/defaultDocument");

            // Build the owin pipeline
            app.UseBuilder(pipelineBuilder);

            // The IRequestRouter is the entry point to the Pages middleware
            var pageRequestRouter = ninject.Get<IRequestRouter>();

            // This is an example of registering an implementation of IPage with a 
            // wildcard request filter and low priority
            pageRequestRouter.Register(
                new FullCustomPage(),
                new FilterAllFilters(
                    new FilterByMethod(Methods.Head, Methods.Get), 
                    new FilterByPath("/pages/*.html")),
                    10);

            // This is an example of routing requests to a class that inherits from the
            // base Page class with an exact match request filter and high priority
            pageRequestRouter.Register(
                ninject.Get<SemiCustomPage>(), 
                new FilterAllFilters(
                    new FilterByMethod(Methods.Get, Methods.Post, Methods.Put),
                    new FilterByPath("/pages/semiCustom.html")),
                    100);

            // The IFluentBuilder provides a mechanism for building elements without
            // writing code that implements the various interfaces like IPage, IComponent etc
            var fluentBuilder = ninject.Get<IFluentBuilder>();

            // You must install build engines before trying to build elements using the
            // fluent builder. You can use the built-in engines, install NuGet packages
            // that provide build engines, or write your own.
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Restful.BuildEngine>().Install(fluentBuilder);

            // This is an example of registering a package containing components, layouts etc
            // that can be referenced by name from other elements. When you register a package
            // like this all of the element in the package are contained in a namespace
            // to avoid naming conflicts. This allows you to install packages from third
            // parties.
            fluentBuilder.Register(ninject.Get<MenuPackage>(), "menus");

            // This is an example of registering all of the elements defined in an assembly
            fluentBuilder.Register(Assembly.GetExecutingAssembly(), t => ninject.Get(t));

            // Now that all of the elements are loaded an registered we can resolve name
            // references between elements
            var nameManager = ninject.Get<INameManager>();
            nameManager.Bind();
        }
    }
}
