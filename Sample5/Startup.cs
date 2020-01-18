#define USER_ACCOUNTS
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

            // Load up the configuration file and watch for changes
            var hostingEnvironment = ninject.Get<IHostingEnvironment>();
            var configFile = new FileInfo(hostingEnvironment.MapPath("config.json"));
            _configurationFileSource = ninject.Get<FileSource>().Initialize(configFile, TimeSpan.FromSeconds(5));

            // Get a reference to the loaded configuration
            var config = ninject.Get<IConfiguration>();

            // Get the Owin Framework pipeline builder
            var pipelineBuilder = ninject.Get<IBuilder>();

#if DEBUG
            // Enable tracing in Prius
            //var priusRepositoryFactory = ninject.Get<Prius.Contracts.Interfaces.Factory.IRepositoryFactory >();
            //priusRepositoryFactory.EnableTracing(ninject.Get<Prius.Contracts.Interfaces.External.ITraceWriterFactory>());

            // Enable tracing in the Owin middleware pipeline
            pipelineBuilder.EnableTracing();
#endif

            // Add middleware defined by this application
            pipelineBuilder.Register(ninject.Get<RedirectMiddleware>()).ConfigureWith(config, "/middleware/redirect");

            // The Pages middleware provides html page rendering and REST services
            pipelineBuilder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/middleware/pages");

            // The Less middleware will dynamically compile less into css and cache in memory
            pipelineBuilder.Register(ninject.Get<OwinFramework.Less.LessMiddleware>()).ConfigureWith(config, "/middleware/less").As("Less");

            // The static files middleware will serve files from the file system to the browser
            pipelineBuilder.Register(ninject.Get<OwinFramework.StaticFiles.StaticFilesMiddleware>()).ConfigureWith(config, "/middleware/staticFiles/scripts").As("Scripts").RunAfter("Less");
            pipelineBuilder.Register(ninject.Get<OwinFramework.StaticFiles.StaticFilesMiddleware>()).ConfigureWith(config, "/middleware/staticFiles/images").As("Images").RunAfter("Less");

            // The Not Found middleware will return a friendly 404 page
            pipelineBuilder.Register(ninject.Get<OwinFramework.NotFound.NotFoundMiddleware>()).ConfigureWith(config, "/middleware/notFound");

            // The Exception Reporter middleware will catch exceptions in the Owin pipeline and report them
            pipelineBuilder.Register(ninject.Get<OwinFramework.ExceptionReporter.ExceptionReporterMiddleware>()).ConfigureWith(config, "/middleware/exceptions").RunFirst();

            // The Output Cache middleware will cache some assets in memory and also instruct the browser to cache certain assets
            pipelineBuilder.Register(ninject.Get<OwinFramework.OutputCache.OutputCacheMiddleware>()).ConfigureWith(config, "/middleware/outputCache");

            // The Versioning middleware will add a version number to assets allowing them to be cached by the browser indefinately
            pipelineBuilder.Register(ninject.Get<OwinFramework.Versioning.VersioningMiddleware>()).ConfigureWith(config, "/middleware/versioning");

# if USER_ACCOUNTS
            // The In Process Session middleware will maintain sessions when their is only 1 web server or if you have sticky sessions.
            // If you have multiple web servers without sticky sessions then you should switch to the CacheSessionMidleware or provide your own
            pipelineBuilder.Register(ninject.Get<OwinFramework.Session.CacheSessionMiddleware>()).ConfigureWith(config, "/middleware/session");

            // The Form Identification middleware will handle POST requests for login, logout, reset password etc
            pipelineBuilder.Register(ninject.Get<OwinFramework.FormIdentification.FormIdentificationMiddleware>()).ConfigureWith(config, "/middleware/identification/forms"); ;

            // The Authorization middleware will allow other middleware to define required permissions
            pipelineBuilder.Register(ninject.Get<OwinFramework.Authorization.AuthorizationMiddleware>()).ConfigureWith(config, "/middleware/authorization");

            // The Authorization UI middleware will allow you to manage user groups, roles and permissions on your website
            pipelineBuilder.Register(ninject.Get<OwinFramework.Authorization.UI.AuthorizationApiMiddleware>()).ConfigureWith(config, "/middleware/authorization/ui");
            pipelineBuilder.Register(ninject.Get<OwinFramework.Authorization.UI.AuthorizationUiMiddleware>()).ConfigureWith(config, "/middleware/authorization/ui");
#endif

#if DEBUG
            // The Debug Info middleware allows you to add ?debug=true to any page rendered by the Pages middleware
            // You can also add ?debug=svg or ?debug=html or ?debug=json
            pipelineBuilder.Register(ninject.Get<DebugInfoMiddleware>()).ConfigureWith(config, "/middleware/pages/debugInfo");
#endif

            // Build the owin pipeline
            app.UseBuilder(pipelineBuilder);

            var nameManager = ninject.Get<INameManager>();
            var fluentBuilder = ninject.Get<IFluentBuilder>();
            Func<Type, object> factory = t => ninject.Get(t);

            // Install the build engine for packages, modules and data providers
            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);

            // Install the build engine for web pages, layouts and regions
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);

            // Install the build engine for REST services
            ninject.Get<OwinFramework.Pages.Restful.BuildEngine>().Install(fluentBuilder);

            // Register packages containing resuable page elements
            fluentBuilder.RegisterPackage(ninject.Get<MenuPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<LayoutsPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<LibrariesPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<TemplatesPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<AjaxPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<DataPackage>(), factory);
            fluentBuilder.RegisterPackage(ninject.Get<TextEffectsPackage>(), factory);

            // Reflect over the executing assembly and register all elements that it contains
            fluentBuilder.Register(Assembly.GetExecutingAssembly(), factory);

            // Load templates from the local file system
            var fileSystemLoader = ninject.Get<FileSystemLoader>();
            fileSystemLoader.TemplateDirectory = new DirectoryInfo(hostingEnvironment.MapPath("~/web/templates"));
#if DEBUG
            fileSystemLoader.ReloadInterval = TimeSpan.FromSeconds(3);
#endif

            // Construct the template parsers that we need
            var asIsParser = ninject.Get<AsIsParser>();
            var mustacheParser = ninject.Get<MustacheParser>();

            // Parse html files using mustache syntax. This allows them to contain data binding expressions
            fileSystemLoader.Load(mustacheParser, p => p.Value.EndsWith(".html"));

            // Parse JavaScript and Vue views without modification
            fileSystemLoader.Load(asIsParser, p => p.Value.EndsWith(".js") || p.Value.EndsWith(".vue"));

            // Build the website
            nameManager.Bind();
        }
    }
}
