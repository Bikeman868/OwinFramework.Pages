using System;
using System.Reflection;
using Ioc.Modules;
using Ninject;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Pages.Core;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Templates;
using OwinFramework.Pages.DebugMiddleware;
using OwinFramework.Pages.Html.Runtime;

namespace Sample3.UseCase6
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

            pipelineBuilder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/sample6/pages");
            pipelineBuilder.Register(ninject.Get<DebugInfoMiddleware>()).ConfigureWith(config, "/sample6/debugInfo");
            app.UseBuilder(pipelineBuilder);

            #endregion

            #region Initialize the Pages middleware

            var fluentBuilder = ninject.Get<IFluentBuilder>();
            var nameManager = ninject.Get<INameManager>();
            var templateBuilder = ninject.Get<ITemplateBuilder>();

            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);

            Func<Type, object> factory = t => ninject.Get(t);

            fluentBuilder.Register(typeof(TwoColumnLeftColumnRegion), factory);
            fluentBuilder.Register(typeof(TwoColumnRightColumnRegion), factory);
            fluentBuilder.Register(typeof(TwoColumnLayout), factory);
            fluentBuilder.Register(typeof(CompanySearchResultsRegion), factory);
            fluentBuilder.Register(typeof(CompanySearchResultsPage), factory);
            fluentBuilder.Register(typeof(BodyRegion), factory);
            fluentBuilder.Register(typeof(MasterPageLayout), factory);
            fluentBuilder.Register(typeof(ApplicationPackage), factory);
            fluentBuilder.Register(typeof(ApplicationModule), factory);

            nameManager.Register(
                templateBuilder.BuildUpTemplate()
                .AddHtml("<p>This is a test</p>")
                .Build(), "/pages/test");

            nameManager.Bind();

            #endregion
        }
    }

    [IsRegion("two_column_left_region")]
    [Container("div", "{ns}_column_left")]
    internal class TwoColumnLeftColumnRegion : ApplicationElement { }

    [IsRegion("two_column_right_region")]
    [Container("div", "{ns}_column_right")]
    internal class TwoColumnRightColumnRegion : ApplicationElement { }

    [IsLayout("two_column_layout", "left_column,right_column")]
    [Container("div", "{ns}_two_col_layout", "{ns}_row")]
    [ZoneRegion("left_column", "two_column_left_region")]
    [ZoneRegion("right_column", "two_column_right_region")]
    internal class TwoColumnLayout : ApplicationElement { }

    [IsRegion("company_results_region")]
    [UsesLayout("two_column_layout")]
    [ZoneTemplate("left_column", "/pages/test")]
    [ZoneTemplate("right_column", "/pages/test")]
    internal class CompanySearchResultsRegion : ApplicationElement { }

    [IsPage("company_results_page")]
    [Route("/")]
    [ZoneRegion("body_zone", "company_results_region")]
    internal class CompanySearchResultsPage : MasterPage
    {
        public CompanySearchResultsPage(IPageDependenciesFactory dependencies) 
            : base(dependencies)
        {
        }
    }

    // ==========================================================================================

    [IsRegion("body_region")]
    [Container("div", "{ns}_content")]
    internal class BodyRegion : ApplicationElement { }

    [IsLayout("master_page_layout", "body_zone")]
    [Container("div", "{ns}_page")]
    [ZoneRegion("body_zone", "body_region")]
    internal class MasterPageLayout : ApplicationElement { }

    [UsesLayout("master_page_layout")]
    [PartOf("usecase6")]
    [DeployedAs("usecase6")]
    internal class MasterPage : Page
    {
        public MasterPage(IPageDependenciesFactory dependencies) 
            : base(dependencies)
        {
            TitleFunc = rc => "Use Case 6";
            BodyClassNames = "uc6_page";
            BodyId = Guid.NewGuid().ToShortString();
        }

        public override IWriteResult WriteHeadArea(IRenderContext context)
        {
            context.Html.WriteUnclosedElement(
                "meta", 
                "name", "description", 
                "content", "This is use case 6");
            context.Html.WriteLine();

            context.Html.WriteUnclosedElement(
                "meta", 
                "name", "keywords", 
                "content", "Use case,sample,demo");
            context.Html.WriteLine();

            context.Html.WriteUnclosedElement(
                "meta", 
                "name", "author", 
                "content", "Martin Halliday");
            context.Html.WriteLine();

            context.Html.WriteUnclosedElement(
                "meta", 
                "name", "viewport", 
                "content", "width=device-width, initial-scale=1.0");
            context.Html.WriteLine();

            return base.WriteHeadArea(context);
        }
    }

    // ==========================================================================================

    [IsPackage("usecase6", "uc6")]
    internal class ApplicationPackage { }

    [IsModule("usecase6", AssetDeployment.InPage)]
    internal class ApplicationModule { }

    [PartOf("usecase6")]
    [DeployedAs("usecase6")]
    internal class ApplicationElement { }
}