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

namespace Sample3.UseCase4
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

            fluentBuilder.Register(typeof(ApplicationPackage), t => ninject.Get(t));
            fluentBuilder.Register(typeof(GameConsole), t => ninject.Get(t));
            fluentBuilder.Register(typeof(Console), t => ninject.Get(t));
            fluentBuilder.Register(typeof(FullWidth), t => ninject.Get(t));
            fluentBuilder.Register(typeof(GameMap), t => ninject.Get(t));
            fluentBuilder.Register(typeof(GameDataProvider), t => ninject.Get(t));

            nameManager.Bind();

            #endregion
        }
    }

    [IsPackage("usecase4", "uc4")]
    internal class ApplicationPackage { }

    [IsPage("console")]
    [PartOf("usecase4")]
    [Route("/", Method.Get)]
    [UsesLayout("console")]
    internal class GameConsole { }

    [IsLayout("console", "map,terminal")]
    [PartOf("usecase4")]
    [ZoneRegion("map", "full_width")]
    [ZoneRegion("terminal", "full_width")]
    [ZoneRegionmap", "game_map")]
    [ZoneHtml("terminal", "", "This is where the terminal window goes")]
    internal class Console { }

    [IsRegion("full_width")]
    [PartOf("usecase4")]
    [Container("div", "{ns}_full_width")]
    [DeployCss("div.{ns}_full_width", "width: 100%; background-color: whitesmoke;")]
    internal class FullWidth { }

    [IsComponent("game_map")]
    [PartOf("usecase4")]
    [NeedsData(typeof(Game))]
    [DeployFunction(null, "reloadMap", "e", "e.src=e.src;")]
    internal class GameMap : Component
    {
        public GameMap(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        {
        }

        public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Body)
            {
                var game = context.Data.Get<Game>();
                if (game != null)
                {
                    var ns = Package.NamespaceName;
                    context.Html.WriteUnclosedElement(
                        "img", 
                        "class", ns + "_map", 
                        "src", "/map/game?g=" + game.GameId, 
                        "onload", "ns." + ns + ".reloadMap();");
                    context.Html.WriteLine();
                }
            }

            return WriteResult.Continue();
        }
    }

    public class Game
    {
        public string GameId;
    }

    [IsDataProvider(typeof(Game))]
    [PartOf("usecase4")]
    public class GameDataProvider : DataProvider
    {
        public GameDataProvider(
            IDataProviderDependenciesFactory dependencies)
            : base(dependencies)
        {
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            var gameId = renderContext.OwinContext.Request.Query["g"];
            var game = new Game { GameId = gameId };
            dataContext.Set(game);
        }
    }
}