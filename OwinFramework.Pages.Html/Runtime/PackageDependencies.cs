using Microsoft.Owin;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class PackageDependencies: IPackageDependencies
    {
        public IRenderContext RenderContext { get; private set; }

        public IAssetManager AssetManager { get; private set; }
        public INameManager NameManager { get; private set; }

        public IModuleDependenciesFactory ModuleDependenciesFactory { get; private set; }
        public IPageDependenciesFactory PageDependenciesFactory { get; private set; }
        public ILayoutDependenciesFactory LayoutDependenciesFactory { get; private set; }
        public IRegionDependenciesFactory RegionDependenciesFactory { get; private set; }
        public IComponentDependenciesFactory ComponentDependenciesFactory { get; private set; }
        public IDataProviderDependenciesFactory DataProviderDependenciesFactory { get; private set; }

        public PackageDependencies(
            IRenderContext renderContext,
            IAssetManager assetManager,
            INameManager nameManager,
            IModuleDependenciesFactory moduleDependenciesFactory,
            IPageDependenciesFactory pageDependenciesFactory,
            ILayoutDependenciesFactory layoutDependenciesFactory,
            IRegionDependenciesFactory regionDependenciesFactory,
            IComponentDependenciesFactory componentDependenciesFactory,
            IDataProviderDependenciesFactory dataProviderDependenciesFactory)
        {
            RenderContext = renderContext;
            AssetManager = assetManager;
            NameManager = nameManager;
            ModuleDependenciesFactory = moduleDependenciesFactory;
            PageDependenciesFactory = pageDependenciesFactory;
            LayoutDependenciesFactory = layoutDependenciesFactory;
            RegionDependenciesFactory = regionDependenciesFactory;
            ComponentDependenciesFactory = componentDependenciesFactory;
            DataProviderDependenciesFactory = dataProviderDependenciesFactory;
        }

        public IPackageDependencies Initialize(IOwinContext context)
        {
            RenderContext.Initialize(context);
            return this;
        }

        public void Dispose()
        {
            RenderContext.Dispose();
        }
    }
}
