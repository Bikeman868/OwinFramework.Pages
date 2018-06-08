using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.RequestFilters;
using OwinFramework.Pages.Facilities.Runtime;

namespace OwinFramework.Pages.Facilities.Builders
{
    internal class PageBuilder: IPageBuilder
    {
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;
        private readonly IPageDependenciesFactory _pageDependenciesFactory;

        public PageBuilder(
            IRequestRouter requestRouter,
            INameManager nameManager,
            IPageDependenciesFactory pageDependenciesFactory)
        {
            _requestRouter = requestRouter;
            _nameManager = nameManager;
            _pageDependenciesFactory = pageDependenciesFactory;
        }

        public IPageDefinition Page(Type declaringType)
        {
            return new PageDefinition(
                declaringType, 
                _requestRouter, 
                _nameManager,
                _pageDependenciesFactory);
        }

        private class PageDefinition: IPageDefinition
        {
            private readonly IRequestRouter _requestRouter;
            private readonly INameManager _nameManager;
            private readonly Webpage _page;
            private readonly Type _declaringType;

            private IRequestFilter _filter;
            private int _filterPriority;
            private string _path;
            private Methods[] _methods;

            public PageDefinition(
                Type declaringType,
                IRequestRouter requestRouter,
                INameManager nameManager,
                IPageDependenciesFactory pageDependenciesFactory)
            {
                _declaringType = declaringType;
                _requestRouter = requestRouter;
                _nameManager = nameManager;
                _page = new Webpage(pageDependenciesFactory.Create());
            }

            IPageDefinition IPageDefinition.Name(string name)
            {
                _page.Name = name;
                return this;
            }

            IPageDefinition IPageDefinition.PartOf(IPackage package)
            {
                _page.Package = package;
                return this;
            }

            IPageDefinition IPageDefinition.PartOf(string packageName)
            {
                _page.Package = _nameManager.ResolvePackage(packageName);

                if (_page.Package == null)
                    throw new PageBuilderException(
                        "Package names must be registered before pages can refer to them. " +
                        "There is no registered package '" + packageName + "'");
                return this;
            }

            IPageDefinition IPageDefinition.AssetDeployment(AssetDeployment assetDeployment)
            {
                _page.AssetDeployment = assetDeployment;
                return this;
            }

            IPageDefinition IPageDefinition.Module(IModule module)
            {
                _page.Module = module;
                return this;
            }

            IPageDefinition IPageDefinition.Module(string moduleName)
            {
                _nameManager.AddResolutionHandler(
                    () => _page.Module = _nameManager.ResolveModule(moduleName));
                return this;
            }

            IPageDefinition IPageDefinition.Path(string path)
            {
                _path = path;
                return this;
            }

            IPageDefinition IPageDefinition.Methods(params Methods[] methods)
            {
                _methods = methods;
                return this;
            }

            IPageDefinition IPageDefinition.RequestFilter(IRequestFilter filter, int priority)
            {
                _filter = filter;
                _filterPriority = priority;
                return this;
            }

            IPageDefinition IPageDefinition.Layout(ILayout layout)
            {
                _page.Layout = layout;
                return this;
            }

            IPageDefinition IPageDefinition.Layout(string name)
            {
                _nameManager.AddResolutionHandler(
                    () => _page.Layout = _nameManager.ResolveLayout(name, _page.Package));
                return this;
            }

            IPageDefinition IPageDefinition.Component(string regionName, IComponent component)
            {
                _page.PopulateRegion(regionName, component);
                return this;
            }

            IPageDefinition IPageDefinition.Component(string regionName, string componentName)
            {
                _nameManager.AddResolutionHandler(
                    () => _page.PopulateRegion(regionName, _nameManager.ResolveComponent(componentName, _page.Package)));
                return this;
            }

            IPageDefinition IPageDefinition.RegionLayout(string regionName, ILayout layout)
            {
                _page.PopulateRegion(regionName, layout);
                return this;
            }

            IPageDefinition IPageDefinition.RegionLayout(string regionName, string layoutName)
            {
                _nameManager.AddResolutionHandler(
                    () => _page.PopulateRegion(regionName, _nameManager.ResolveLayout(layoutName, _page.Package)));
                return this;
            }

            IPage IPageDefinition.Build()
            {
                if (_filter == null)
                {
                    if (_methods == null)
                    {
                        _filter = new FilterByPath(_path);
                    }
                    else
                    {
                        if (_path == null)
                        {
                            _filter = new FilterByMethod(_methods);
                        }
                        else
                        {
                            _filter = new FilterAllFilters(new FilterByMethod(_methods), new FilterByPath(_path));
                        }
                    }
                }

                _requestRouter.Register(_page, _filter, _filterPriority, _declaringType);

                return _page;
            }
        }

        private class Webpage : Page
        {
            public AssetDeployment AssetDeployment { get; set; }
            public IModule Module { get; set; }

            public Webpage(IPageDependencies dependencies)
                : base(dependencies)
            {
            }

            public void PopulateRegion(string regionName, IComponent component)
            {
            }

            public void PopulateRegion(string regionName, ILayout layout)
            {
            }

            public override Task Run(IOwinContext context)
            {
                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("Not implemented yet");
            }
        }

    }
}
