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
using OwinFramework.Pages.CMS.Runtime;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Templates;
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.DebugMiddleware;
using OwinFramework.Pages.Html.Templates;
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
            pipelineBuilder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/sample1/pages");
            pipelineBuilder.Register(ninject.Get<DebugInfoMiddleware>()).ConfigureWith(config, "/sample1/pages/debugInfo");
            pipelineBuilder.Register(ninject.Get<OwinFramework.NotFound.NotFoundMiddleware>()).ConfigureWith(config, "/sample1/notFound");
            pipelineBuilder.Register(ninject.Get<OwinFramework.Documenter.DocumenterMiddleware>()).ConfigureWith(config, "/sample1/documenter").RunFirst();
            pipelineBuilder.Register(ninject.Get<OwinFramework.DefaultDocument.DefaultDocumentMiddleware>()).ConfigureWith(config, "/sample1/defaultDocument");
            pipelineBuilder.Register(ninject.Get<OwinFramework.AnalysisReporter.AnalysisReporterMiddleware>()).ConfigureWith(config, "/sample1/analysisReporter");
            pipelineBuilder.Register(ninject.Get<OwinFramework.ExceptionReporter.ExceptionReporterMiddleware>()).RunFirst();

            // Build the owin pipeline
            app.UseBuilder(pipelineBuilder);

            // The IRequestRouter is the entry point to the Pages middleware
            var requestRouter = ninject.Get<IRequestRouter>();

            // The name manager allows elements that reference each other by name
            // to be registered in any order
            var nameManager = ninject.Get<INameManager>();

            // The Fluent Builder provides a mechanism for building elements (pages,
            // regions, layouts, components, data providers etc) without writing code 
            // that implements the various interfaces like IPage, IComponent etc
            var fluentBuilder = ninject.Get<IFluentBuilder>();

            // This is an example of registering an implementation of IPage with a 
            // wildcard request filter and below normal priority
            requestRouter.Register(
                new FullCustomPage(),
                new FilterAllFilters(
                    new FilterByMethod(Method.Head, Method.Get), 
                    new FilterByPath("/pages/*.html")),
                    -10);

            // This is an example of routing requests to a class that inherits from the
            // base Page class with an exact match request filter and higher than normal
            // priority
            requestRouter.Register(
                ninject.Get<SemiCustomPage>(), 
                new FilterAllFilters(
                    new FilterByMethod(Method.Get, Method.Post, Method.Put),
                    new FilterByPath("/pages/semiCustom.html")),
                    100);

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
            fluentBuilder.Register(ninject.Get<MenuPackage>(), "menus");
            fluentBuilder.Register(ninject.Get<LayoutsPackage>(), "layouts");
            fluentBuilder.Register(ninject.Get<LibrariesPackage>(), "libraries");
            fluentBuilder.Register(ninject.Get<CmsStaticRuntimePackage>(), "cms");

            // This is an example of registering all of the elements defined in an assembly
            fluentBuilder.Register(Assembly.GetExecutingAssembly(), t => ninject.Get(t));

            // This is an example of building and registering a custom template
            var templateBuilder = ninject.Get<ITemplateBuilder>();

            var template1 = templateBuilder.BuildUpTemplate()
                .AddElementOpen("p", "class", "dummy")
                .AddText("this-is-the", "This is the ")
                .AddDataField<ApplicationInfo>(a => a.Name)
                .AddText("application", " application")
                .AddElementClose()
                .Build();
            nameManager.Register(template1, "/common/pageTitle");

            var template2 = templateBuilder.BuildUpTemplate()
                .AddHtml("alert('Hello!');")
                .Build();
            nameManager.Register(template2, "/common/pageInitialization");

            var template3 = templateBuilder.BuildUpTemplate()
                .AddElementOpen("p", "class", "test")
                .AddText("page-2-body", "Page 2 body")
                .AddElementClose()
                .Build();
            nameManager.Register(template3, "/page2/body");

            // This is an example of loading and parsing template files using different
            // parsers for different file formats
            var asIsTemplateParser = ninject.Get<AsIsParser>();
            var markdownTemplateParser = ninject.Get<MarkdownParser>();
            var mustacheParser = ninject.Get<MustacheParser>();
            var multiPartParser = ninject.Get<MultiPartParser>();

            // This is an example of loading and parsing template files using different
            // parsers for different file formats
            var fileSystemLoader = ninject.Get<FileSystemLoader>();
            fileSystemLoader.RootPath = new PathString("/file");
            fileSystemLoader.ReloadInterval = TimeSpan.FromSeconds(3);
            fileSystemLoader.Load(asIsTemplateParser, p => p.Value.EndsWith(".html"));
            fileSystemLoader.Load(markdownTemplateParser, p => p.Value.EndsWith(".md"));
            fileSystemLoader.Load(mustacheParser, p => p.Value.EndsWith(".svg"));
            fileSystemLoader.Load(multiPartParser, p => p.Value.EndsWith(".vue"));

            // This is an example of loading and parsing template from a URL
            var uriLoader = ninject.Get<UriLoader>();
            uriLoader.LoadUri(new Uri("https://raw.githubusercontent.com/Bikeman868/OwinFramework.Middleware/master/OwinFramework.FormIdentification/readme.md"), markdownTemplateParser, "/url/template1");

            // Now that all of the elements are loaded an registered we can resolve name
            // references between elements
            nameManager.Bind();
        }
    }
}
