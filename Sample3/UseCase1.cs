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
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.DebugMiddleware;
using OwinFramework.Pages.Framework.DataModel;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Runtime;

namespace Sample3.UseCase1
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
            var requestRouter = ninject.Get<IRequestRouter>();
            var nameManager = ninject.Get<INameManager>();

            ninject.Get<OwinFramework.Pages.Html.BuildEngine>().Install(fluentBuilder);
            ninject.Get<OwinFramework.Pages.Framework.BuildEngine>().Install(fluentBuilder);
            
            fluentBuilder.Register(Assembly.GetExecutingAssembly(), t => ninject.Get(t));

            nameManager.Bind();

            // This code dynamically adds a route from the root path to the 'page1' page from the 'usecase1' package
            // In a regular application these routes would be defined statically as attributes on the pages. This
            // project is unusual because it contains multiple use cases which are essentially all different websites
            // built into one assembly
            var package = nameManager.ResolvePackage("usecase1");
            var page1 = nameManager.ResolvePage("page1", package);
            requestRouter.Register(
                page1,
                new FilterAllFilters(
                    new FilterByMethod(Methods.Head, Methods.Get),
                    new FilterByPath("/")),
                    -10);

            #endregion
        }
    }

    /*
     * Page 1 uses the standard page layout but defines content
     * that is specific to page 1
     */

    [IsPage("page1")]
    [Route("/uc1/page1", Methods.Get)]
    [RegionComponent("body", "page1_body")]
    internal class Page1 : PageBase { }

    [IsComponent("page1_body")]
    [RenderHtml("page1.body", "<p>This is page 1</p>")]
    internal class Page1BodyComponent : ApplicationElement { }

    /*
     * Page 2 uses the standard page layout but defines content
     * that is specific to page 2
     */

    [IsPage("page2")]
    [Route("/uc1/page2", Methods.Get)]
    [RegionLayout("body", "page2_body")]
    internal class Page2 : PageBase { }

    [IsLayout("page2_body", "main")]
    [LayoutRegion("main", "page2_body")]
    [RegionComponent("main", "address")]
    internal class Page2BodyLayout : ApplicationElement { }

    [IsRegion("page2_body")]
    [Repeat(typeof(Person))]
    [NeedsData("person_address")]
    internal class Page2BodyRegion : ApplicationElement { }

    #region Define page layout that is shared by all pages

    [UsesLayout("layout")]
    internal class PageBase : ApplicationElement { }

    [IsLayout("layout", "header,body,footer")]
    [LayoutRegion("header", "header")]
    [LayoutRegion("body", "body")]
    [LayoutRegion("footer", "footer")]
    [RegionComponent("header", "header")]
    [RegionComponent("footer", "footer")]
    internal class BasePageLayout : ApplicationElement { }

    [IsRegion("header")]
    [Container("div", "{ns}_header-region")]
    internal class HeaderRegion : ApplicationElement { }

    [IsRegion("body")]
    [Container("div", "{ns}_body-region")]
    internal class BodyRegion : ApplicationElement { }

    [IsRegion("footer")]
    [Container("div", "{ns}_footer-region")]
    internal class FooterRegion : ApplicationElement { }

    [IsComponent("header")]
    [RenderHtml("header", "<p>Header</p>")]
    internal class HeaderComponent : ApplicationElement { }

    // Note that we do not define a body component here because
    // every page has a different one.

    [IsComponent("footer")]
    [RenderHtml("footer", "<p>Footer</p>")]
    internal class FooterComponent : ApplicationElement { }

    #endregion

    #region Define an application namespace

    [IsPackage("usecase1", "uc1")]
    internal class ApplicationPackage { }

    [PartOf("usecase1")]
    internal class ApplicationElement { }

    #endregion

    #region Define some test data to work with

    internal class Person
    {
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    internal class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }

    [IsDataProvider("people", typeof(IList<Person>))]
    [PartOf("usecase1")]
    internal class PersonListProvider : DataProvider
    {
        public PersonListProvider(IDataProviderDependenciesFactory dependencies)
            : base(dependencies) { }

        protected override void Supply(IRenderContext renderContext, IDataContext dataContext, IDataDependency dependency)
        {
            var people = new[]
            {
                new Person { Name = "Person 1", Address = new Address { Street = "1 Main St", City = "City", ZipCode = "12345" }},
                new Person { Name = "Person 2", Address = new Address { Street = "2 Main St", City = "City", ZipCode = "54321" }},
                new Person { Name = "Person 3", Address = new Address { Street = "3 Main St", City = "City", ZipCode = "99999" }},
            };
            dataContext.Set<IList<Person>>(people);
        }
    }

    [IsDataProvider("person_address")]
    [PartOf("usecase1")]
    [SuppliesData(typeof(Address))]
    [NeedsData(typeof(Person))]
    internal class PersonAddressProvider : DataProvider
    {
        public PersonAddressProvider(IDataProviderDependenciesFactory dependencies)
            : base(dependencies) { }

        protected override void Supply(IRenderContext renderContext, IDataContext dataContext, IDataDependency dependency)
        {
            var person = dataContext.Get<Person>();
            dataContext.Set(person.Address);
        }
    }

    [IsComponent("person")]
    [PartOf("usecase1")]
    [NeedsData(typeof(Person))]
    internal class PersonComponent : Component
    {
        public PersonComponent(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        {
        }

        public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Body)
            {
                var person = context.Data.Get<Person>();
                context.Html.WriteElementLine("p", person.Name);
                context.Html.WriteOpenTag("hr", true);
                context.Html.WriteLine();
            }
            return WriteResult.Continue();
        }
    }

    [IsComponent("address")]
    [PartOf("usecase1")]
    [NeedsData(typeof(Address))]
    internal class AddressComponent : Component
    {
        public AddressComponent(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        {
        }

        public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Body)
            {
                var address = context.Data.Get<Address>();
                context.Html.WriteElementLine("p", address.Street);
                context.Html.WriteElementLine("p", address.City);
                context.Html.WriteElementLine("p", address.ZipCode);
                context.Html.WriteOpenTag("hr", true);
                context.Html.WriteLine();
            }
            return WriteResult.Continue();
        }
    }

    #endregion
}