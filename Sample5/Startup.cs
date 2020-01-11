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
using OwinFramework.Pages.DebugMiddleware;
using OwinFramework.Pages.Html.Templates;
using OwinFramework.Pages.Standard;
using Urchin.Client.Sources;
using OwinFramework.Pages.Core;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using Sample5.Middleware;

[assembly: OwinStartup(typeof(Sample5.Startup))]

namespace Sample5
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

            // Prius requires a factory to work
            PriusIntegration.PriusFactory.Ninject = ninject;

            // Next we load up the configuration file and watch for changes
            var hostingEnvironment = ninject.Get<IHostingEnvironment>();
            var configFile = new FileInfo(hostingEnvironment.MapPath("config.json"));
            _configurationFileSource = ninject.Get<FileSource>().Initialize(configFile, TimeSpan.FromSeconds(5));

            // Get a reference to the loaded configuration
            var config = ninject.Get<IConfiguration>();

            // Get the Owin Framework pipeline builder
            var pipelineBuilder = ninject.Get<IBuilder>();
#if DEBUG
            pipelineBuilder.EnableTracing();
#endif

            // Define the middleware to add to the Owin Pipeline
            pipelineBuilder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/sample5/pages");
            pipelineBuilder.Register(ninject.Get<RedirectMiddleware>()).ConfigureWith(config, "/sample5/redirect");
            pipelineBuilder.Register(ninject.Get<OwinFramework.Less.LessMiddleware>()).ConfigureWith(config, "/sample5/less").As("Less");
            pipelineBuilder.Register(ninject.Get<OwinFramework.StaticFiles.StaticFilesMiddleware>()).ConfigureWith(config, "/sample5/staticFiles/scripts").As("Scripts").RunAfter("Less");
            pipelineBuilder.Register(ninject.Get<OwinFramework.StaticFiles.StaticFilesMiddleware>()).ConfigureWith(config, "/sample5/staticFiles/images").As("Images").RunAfter("Less");
            pipelineBuilder.Register(ninject.Get<OwinFramework.NotFound.NotFoundMiddleware>()).ConfigureWith(config, "/sample5/notFound");
            pipelineBuilder.Register(ninject.Get<OwinFramework.ExceptionReporter.ExceptionReporterMiddleware>()).RunFirst();

#if DEBUG
            pipelineBuilder.Register(ninject.Get<DebugInfoMiddleware>()).ConfigureWith(config, "/sample5/pages/debugInfo");
#endif

            // Build the owin pipeline
            app.UseBuilder(pipelineBuilder);

            var nameManager = ninject.Get<INameManager>();
            var fluentBuilder = ninject.Get<IFluentBuilder>();
            Func<Type, object> factory = t => ninject.Get(t);

            // Install build engine for packages, data providers
            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);

            // Install build engine for web pages
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);

            // Install build engine for REST endpoints
            ninject.Get<OwinFramework.Pages.Restful.BuildEngine>().Install(fluentBuilder);

            // Register packages containing components, layouts etc
            // that can be referenced by name from other elements.
            fluentBuilder.RegisterPackage(ninject.Get<MenuPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<LayoutsPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<LibrariesPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<TemplatesPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<AjaxPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<DataPackage>(), factory);

            // Register all of the elements defined in this assembly
            fluentBuilder.Register(Assembly.GetExecutingAssembly(), t => ninject.Get(t));

            // Load templates from the local file system
            var fileSystemLoader = ninject.Get<FileSystemLoader>();
            fileSystemLoader.TemplateDirectory = new DirectoryInfo(hostingEnvironment.MapPath("~/web/templates"));
#if DEBUG
            fileSystemLoader.ReloadInterval = TimeSpan.FromSeconds(3);
#endif

            // Construct the template parsers that we need
            var asIsParser = ninject.Get<AsIsParser>();
            var mustacheParser = ninject.Get<MustacheParser>();

            // Parse html files using mustache syntax
            fileSystemLoader.Load(mustacheParser, p => p.Value.EndsWith(".html"));

            // Parse Vue.js view models without modification
            fileSystemLoader.Load(asIsParser, p => p.Value.EndsWith(".vue"));

            // Parse JavaScript without modification
            fileSystemLoader.Load(asIsParser, p => p.Value.EndsWith(".js"));

            // Build the website
            nameManager.Bind();
        }
    }
}
