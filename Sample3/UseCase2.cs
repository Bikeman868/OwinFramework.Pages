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
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Templates;
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.DebugMiddleware;
using OwinFramework.Pages.Framework.DataModel;

namespace Sample3.UseCase2
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
            pipelineBuilder.Register(ninject.Get<DebugInfoMiddleware>()).ConfigureWith(config, "/sample3/debugInfo");
            app.UseBuilder(pipelineBuilder);

            #endregion

            #region Initialize the Pages middleware

            var fluentBuilder = ninject.Get<IFluentBuilder>();
            var nameManager = ninject.Get<INameManager>();
            var templateBuilder = ninject.Get<ITemplateBuilder>();
            var requestRouter = ninject.Get<IRequestRouter>();

            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            
            fluentBuilder.Register(Assembly.GetExecutingAssembly(), t => ninject.Get(t));

            nameManager.Register(
                templateBuilder.BuildUpTemplate()
                    .AddElementOpen("p", "class", "dummy")
                    .AddText("this-is-the", "This is the ")
                    .AddDataField(typeof(ApplicationInfo), "Name")
                    .AddText("application", " application")
                    .AddElementClose()
                    .Build(), 
                "/common/pageTitle");

            nameManager.Bind();

            // This code dynamically adds a route from the root path to the 'home' page from the 'usecase2' package
            // In a regular application these routes would be defined statically as attributes on the pages. This
            // project is unusual because it contains multiple use cases which are essentially all different websites
            // built into one assembly
            var package = nameManager.ResolvePackage("usecase2");
            var page1 = nameManager.ResolvePage("home", package);
            requestRouter.Register(
                page1,
                new FilterAllFilters(
                    new FilterByMethod(Methods.Head, Methods.Get),
                    new FilterByPath("/")),
                    -10);
            #endregion
        }
    }

    [IsPage("home")]
    [Route("/uc2", Methods.Get)]
    [UsesLayout("layout")]
    internal class Page1 : ApplicationElement { }

    [IsLayout("layout", "body")]
    [LayoutRegion("body", "body")]
    [RegionTemplate("body", "/common/pageTitle")]
    internal class BasePageLayout : ApplicationElement { }

    [IsRegion("body")]
    internal class BodyRegion : ApplicationElement { }

    [IsPackage("usecase2", "uc2")]
    internal class ApplicationPackage { }

    [PartOf("usecase2")]
    internal class ApplicationElement { }

    #region Define some test data to work with

    internal class ApplicationInfo
    {
        public string Name { get { return "My Application"; } }
    }

    [IsDataProvider("application", typeof(ApplicationInfo))]
    internal class ApplicationInfoDataProvider : DataProvider
    {
        private readonly ApplicationInfo _applicationInfo;

        public ApplicationInfoDataProvider(IDataProviderDependenciesFactory dependencies)
            : base(dependencies)
        {
            _applicationInfo = new ApplicationInfo();
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            dataContext.Set(_applicationInfo);
        }
    }

    #endregion
}