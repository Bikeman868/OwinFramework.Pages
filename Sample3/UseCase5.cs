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
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Templates;
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.DebugMiddleware;
using OwinFramework.Pages.Framework.DataModel;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Runtime;
using OwinFramework.Pages.Html.Templates;

namespace Sample3.UseCase5
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
            app.UseBuilder(pipelineBuilder);

            #endregion

            #region Initialize the Pages middleware

            var fluentBuilder = ninject.Get<IFluentBuilder>();
            var requestRouter = ninject.Get<IRequestRouter>();
            var nameManager = ninject.Get<INameManager>();

            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);
            
            // We would not normally register the classes individually like this,
            // but I want to keep the use cases separate and in this version the
            // fluent builder can not register types by namespace.

            fluentBuilder.Register(typeof(ApplicationModule), t => ninject.Get(t));
            fluentBuilder.Register(typeof(ApplicationPackage), t => ninject.Get(t));
            fluentBuilder.Register(typeof(HomePage), t => ninject.Get(t));
            fluentBuilder.Register(typeof(HomeLayout), t => ninject.Get(t));
            
            var fileSystemLoader = ninject.Get<FileSystemLoader>();
            fileSystemLoader.Package = nameManager.ResolvePackage("usecase5");
            fileSystemLoader.Module = nameManager.ResolveModule("usecase5");

            var parser = ninject.Get<ComponentParser>();
            parser.RenderIntoPage = false;

            fileSystemLoader.Load(parser, p => p.Value.EndsWith("test2.css") || p.Value.EndsWith("test2.html"));

            nameManager.Bind();

            #endregion
        }
    }

    [IsModule("usecase5", AssetDeployment.PerModule)]
    internal class ApplicationModule { }

    [IsPackage("usecase5", "uc5")]
    internal class ApplicationPackage : OwinFramework.Pages.Core.Interfaces.IPackage
    {
        public ElementType ElementType { get { return ElementType.Package; } }
        public string NamespaceName { get; set; }
        public IModule Module { get; set; }
        public string Name { get; set; }

        private ITemplateBuilder _templateBuilder;
        private INameManager _nameManager;

            public ApplicationPackage(
            ITemplateBuilder templateBuilder,
            INameManager nameManager)
        {
            _templateBuilder = templateBuilder;
            _nameManager = nameManager;
        }

        public OwinFramework.Pages.Core.Interfaces.IPackage Build(IFluentBuilder fluentBuilder)
        {
            Module = _nameManager.ResolveModule("usecase5");

            var template = _templateBuilder
                .BuildUpTemplate()
                .PartOf(this)
                .DeployIn(Module)
                .AddStaticCss("body { background-color: whitesmoke; color: darkblue; }")
                .AddHtml("<h1>This is use case 5</h1>")
                .Build();
            _nameManager.Register(template, "/test1");

            return this;
        }
    }

    [IsPage("home")]
    [PartOf("usecase5")]
    [DeployedAs("usecase5")]
    [Route("/", Method.Get)]
    [UsesLayout("home")]
    internal class HomePage { }

    [IsLayout("home", "test1,test2")]
    [PartOf("usecase5")]
    [DeployedAs("usecase5")]
    [ZoneTemplate("test1", "/test1")]
    [ZoneTemplate("test2", "/test2")]
    internal class HomeLayout { }
}