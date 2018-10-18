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
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Templates;
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.DebugMiddleware;
using OwinFramework.Pages.Standard;
using Sample1.SampleDataProviders;
using Sample1.SamplePackages;
using Sample1.SamplePages;
using Urchin.Client.Sources;
using OwinFramework.Pages.Core;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

[assembly: OwinStartup(typeof(Sample1.Startup))]

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
            pipelineBuilder.Register(ninject.Get<OwinFramework.ExceptionReporter.ExceptionReporterMiddleware>()).RunFirst();

            // Build the owin pipeline
            app.UseBuilder(pipelineBuilder);

            // The IRequestRouter is the entry point to the Pages middleware
            var pageRequestRouter = ninject.Get<IRequestRouter>();

            // The name manager allows elements that reference each other by name
            // to be registered in any order
            var nameManager = ninject.Get<INameManager>();

            // The Fluent Builder provides a mechanism for building elements (pages,
            // regions, layouts, components, data providers etc) without writing code 
            // that implements the various interfaces like IPage, IComponent etc
            var fluentBuilder = ninject.Get<IFluentBuilder>();

            // This is an example of registering an implementation of IPage with a 
            // wildcard request filter and below normal priority
            pageRequestRouter.Register(
                new FullCustomPage(),
                new FilterAllFilters(
                    new FilterByMethod(Methods.Head, Methods.Get), 
                    new FilterByPath("/pages/*.html")),
                    -10);

            // This is an example of routing requests to a class that inherits from the
            // base Page class with an exact match request filter and higher than normal
            // priority
            pageRequestRouter.Register(
                ninject.Get<SemiCustomPage>(), 
                new FilterAllFilters(
                    new FilterByMethod(Methods.Get, Methods.Post, Methods.Put),
                    new FilterByPath("/pages/semiCustom.html")),
                    100);

            // You must install build engines before trying to build elements using the
            // fluent builder. You can use the built-in engines, install NuGet packages
            // that provide build engines, or write your own.
            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Restful.BuildEngine>().Install(fluentBuilder);

            // This is an example of registering a package containing components, layouts etc
            // that can be referenced by name from other elements.
            // When you register a package like this all of the element in the package are 
            // contained in a namespace to avoid naming conflicts. The namespace will be used 
            // as a prefix on all css class names and JavaScript function names.
            // The package namespace can also be used to reference the elements within the
            // package by putting the namespace name and a colon in front of the element
            // name. For example after loading the menu package into the "menus" namespace
            // your application can refer to the desktop menu region as "menus:desktop_menu"
            fluentBuilder.Register(ninject.Get<MenuPackage>(), "menus");

            // This is an example of registering all of the elements defined in an assembly
            fluentBuilder.Register(Assembly.GetExecutingAssembly(), t => ninject.Get(t));

            // This is an example of building and registering a custom template
            var templateBuilder = ninject.Get<ITemplateBuilder>();
            var template1 = templateBuilder.BuildUpTemplate()
                .AddElementOpen("p", "class", "dummy")
                .AddText("dummy-text", "This is a dummy")
                //.AddDataField(typeof(ApplicationInfo), "Name")
                .AddElementClose()
                .Build();
            var template2 = templateBuilder.BuildUpTemplate()
                .AddElementOpen("p", "class", "test")
                .AddText("dummy-text", "Page 2 body")
                .AddElementClose()
                .Build();
            nameManager.Register(template1, "/common/pageTitle");
            nameManager.Register(template2, "/page2/body");

            // Now that all of the elements are loaded an registered we can resolve name
            // references between elements
            nameManager.Bind();
        }
    }
}
