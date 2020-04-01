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
using Urchin.Client.Interfaces;

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

            var urchin = ninject.Get<IConfigurationStore>();
            urchin.UpdateConfiguration("{ 'owinFramework': { 'pages': { 'framework': { 'debugLogging': true } } } }");

            var config = ninject.Get<IConfiguration>();
            var pipelineBuilder = ninject.Get<IBuilder>().EnableTracing();

            pipelineBuilder.Register(ninject.Get<PagesMiddleware>()).ConfigureWith(config, "/sample3/pages");
            pipelineBuilder.Register(ninject.Get<DebugInfoMiddleware>()).ConfigureWith(config, "/sample3/debugInfo");
            app.UseBuilder(pipelineBuilder);

            #endregion

            var fluentBuilder = ninject.Get<IFluentBuilder>();
            var nameManager = ninject.Get<INameManager>();
            var templateBuilder = ninject.Get<ITemplateBuilder>();
            var requestRouter = ninject.Get<IRequestRouter>();
            Func<Type, object> factory = t => ninject.Get(t);

            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            
            fluentBuilder.Register(typeof(Page1), factory);
            fluentBuilder.Register(typeof(DivRegion), factory);
            fluentBuilder.Register(typeof(ApplicationPackage), factory);
            fluentBuilder.Register(typeof(BasePageLayout), factory);
            fluentBuilder.Register(typeof(ApplicationInfoDataProvider), factory);
            fluentBuilder.Register(typeof(PersonListProvider), factory);
            fluentBuilder.Register(typeof(PersonAddressProvider), factory);

            nameManager.Register(
                templateBuilder.BuildUpTemplate()
                    .AddElementOpen(PageArea.Body, "p", "class", "dummy")
                    .AddText(PageArea.Body, "this-is-the", "This is the ")
                    .AddDataField<ApplicationInfo>(PageArea.Body, a => a.Name)
                    .AddText(PageArea.Body, "application", " application\n")
                    .AddElementClose(PageArea.Body)
                    .Build(), 
                "/title");

            nameManager.Register(
                templateBuilder.BuildUpTemplate()
                    .AddElementOpen(PageArea.Body, "p")
                    .AddDataField<Address>(PageArea.Body, p => p.Street).AddHtml(PageArea.Body, "<br>")
                    .AddDataField<Address>(PageArea.Body, p => p.City).AddHtml(PageArea.Body, "<br>")
                    .AddDataField<Address>(PageArea.Body, p => p.ZipCode)
                    .AddElementClose(PageArea.Body)
                    .Build(),
                "/address");

            nameManager.Register(
                templateBuilder.BuildUpTemplate()
                    .AddElementOpen(PageArea.Body, "h3")
                    .AddDataField<Person>(PageArea.Body, p => p.Name)
                    .AddElementClose(PageArea.Body)
                    .ExtractProperty<Person>(PageArea.Body, p => p.Address)
                    .AddTemplate("/address")
                    .Build(),
                "/person");

            nameManager.Register(
                templateBuilder.BuildUpTemplate()
                    .RepeatStart<Person>(PageArea.Body)
                    .AddTemplate("/person")
                    .RepeatEnd()
                    .Build(),
                "/people");

            nameManager.Bind();
        }
    }

    [IsPage("home")]
    [Route("/", Method.Get)]
    [UsesLayout("layout")]
    internal class Page1 : ApplicationElement { }

    [IsLayout("layout", "head,body")]
    [ZoneRegion("head", "div")]
    [ZoneRegion("body", "div")]
    [ZoneTemplate("head", "/title")]
    [ZoneTemplate("body", "/people")]
    internal class BasePageLayout : ApplicationElement { }

    [IsRegion("div")]
    internal class DivRegion : ApplicationElement { }

    [IsPackage("usecase2", "uc2")]
    internal class ApplicationPackage { }

    [PartOf("usecase2")]
    internal class ApplicationElement { }
}