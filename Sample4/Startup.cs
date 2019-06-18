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
using OwinFramework.Pages.CMS.Manager;
using OwinFramework.Pages.CMS.Runtime;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.DebugMiddleware;
using OwinFramework.Pages.Html.Templates;
using OwinFramework.Pages.Standard;
using Urchin.Client.Sources;
using OwinFramework.Pages.Core;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

[assembly: OwinStartup(typeof(Sample4.Startup))]

namespace Sample4
{
    public class Startup
    {
        private static IDisposable _configurationFileSource;
        private static IDisposable _updateSyncronizer;

        public void Configuration(IAppBuilder app)
        {
            // The package locator finds assemblies that advertise their IoC requirements
            var packageLocator = new PackageLocator()
                .ProbeBinFolderAssemblies()
                .Add(Assembly.GetExecutingAssembly());

            // For this example we will use Ninject as the IoC container
            var ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));

            // Prius requires a factory to work
            PriusIntegration.PriusFactory.Ninject = ninject;

            // Next we load up the configuration file and watch for changes
            var hostingEnvironment = ninject.Get<IHostingEnvironment>();
            var configFile = new FileInfo(hostingEnvironment.MapPath("config.json"));
            _configurationFileSource = ninject.Get<FileSource>().Initialize(configFile, TimeSpan.FromSeconds(5));

            // Get a reference to the loaded configuration
            var config = ninject.Get<IConfiguration>();

            // Get the Owin Framework pipeline builder
            var pipelineBuilder = ninject.Get<IBuilder>().EnableTracing();

            // Define the middleware to add to the Owin Pipeline
            pipelineBuilder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/sample4/pages");
            pipelineBuilder.Register(ninject.Get<DebugInfoMiddleware>()).ConfigureWith(config, "/sample4/pages/debugInfo");
            pipelineBuilder.Register(ninject.Get<OwinFramework.NotFound.NotFoundMiddleware>()).ConfigureWith(config, "/sample4/notFound");
            pipelineBuilder.Register(ninject.Get<OwinFramework.DefaultDocument.DefaultDocumentMiddleware>()).ConfigureWith(config, "/sample4/defaultDocument");
            pipelineBuilder.Register(ninject.Get<OwinFramework.ExceptionReporter.ExceptionReporterMiddleware>()).RunFirst();

            // Build the owin pipeline
            app.UseBuilder(pipelineBuilder);

            // The name manager allows elements that reference each other by name
            // to be registered in any order
            var nameManager = ninject.Get<INameManager>();

            // The Fluent Builder provides a mechanism for building elements (pages,
            // regions, layouts, components, data providers etc) without writing code 
            // that implements the various interfaces like IPage, IComponent etc
            var fluentBuilder = ninject.Get<IFluentBuilder>();

            // If you want to use dependency injection with your elements then you will need
            // to define a factory method so that the fluent builder can construct the classes
            // that it finds in your assemblies.
            Func<Type, object> factory = t => ninject.Get(t);

            // You must install build engines before trying to build elements using the
            // fluent builder. You can use the built-in engines, install NuGet packages
            // that provide build engines, or write your own.
            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Restful.BuildEngine>().Install(fluentBuilder);

            // This is an example of registering packages containing components, layouts etc
            // that can be referenced by name from other elements.
            // When you register a package like this all of the element in the package are 
            // contained in a namespace to avoid naming conflicts. The namespace will be used 
            // as a prefix on all css class names and JavaScript function names.
            // The package namespace can also be used to reference the elements within the
            // package by putting the namespace name and a colon in front of the element
            // name. For example after loading the menu package into the "menus" namespace
            // your application can refer to the desktop menu region as "menus:desktop_menu"
            // Packages also have a default namespace to use if you don't specify one in
            // your application code.
            fluentBuilder.RegisterPackage(ninject.Get<MenuPackage>(), "menus", factory);
            fluentBuilder.RegisterPackage(ninject.Get<LayoutsPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<LibrariesPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<TemplatesPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<AjaxPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<CmsStaticRuntimePackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<CmsManagerPackage>(), factory);

            // Register all of the elements defined in this assembly
            fluentBuilder.Register(Assembly.GetExecutingAssembly(), t => ninject.Get(t));

            // This is an example of loading and parsing template files using the same
            // parser for all templates. In this case they are html files with server-side
            // data binding expressions using the mustache format.
            var mustacheParser = ninject.Get<MustacheParser>();
            var fileSystemLoader = ninject.Get<FileSystemLoader>();
            fileSystemLoader.ReloadInterval = TimeSpan.FromSeconds(3);
            fileSystemLoader.Load(mustacheParser, p => p.Value.EndsWith(".html"));

            // Now that all of the elements are loaded an registered we can resolve name
            // references between elements binding them together into a runable website
            nameManager.Bind();
        }
    }
}
