using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class PageDefinition : IPageDefinition
    {
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;
        private readonly BuiltPage _page;
        private readonly Type _declaringType;

        public PageDefinition(
            Type declaringType,
            IRequestRouter requestRouter,
            INameManager nameManager,
            IPageDependenciesFactory pageDependenciesFactory,
            IPackage package)
        {
            _declaringType = declaringType;
            _requestRouter = requestRouter;
            _nameManager = nameManager;
            _page = new BuiltPage(pageDependenciesFactory);
            _page.Package = package;
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
            _nameManager.AddResolutionHandler(() =>
            {
                _page.Module = _nameManager.ResolveModule(moduleName);
            });
            return this;
        }

        IPageDefinition IPageDefinition.Route(string path, int priority, params Methods[] methods)
        {
            if (methods == null || methods.Length == 0)
            {
                if (string.IsNullOrEmpty(path))
                    return this;
                _requestRouter.Register(_page, new FilterByPath(path), priority, _declaringType);
            }
            else
            {
                if (string.IsNullOrEmpty(path))
                    _requestRouter.Register(_page, new FilterByMethod(methods), priority, _declaringType);
                else
                    _requestRouter.Register(
                        _page,
                        new FilterAllFilters(
                            new FilterByMethod(methods),
                            new FilterByPath(path)),
                        priority,
                        _declaringType);
            }
            return this;
        }

        IPageDefinition IPageDefinition.Route(IRequestFilter filter, int priority)
        {
            _requestRouter.Register(_page, filter, priority, _declaringType);
            return this;
        }

        IPageDefinition IPageDefinition.Layout(ILayout layout)
        {
            _page.Layout = layout;
            return this;
        }

        IPageDefinition IPageDefinition.Layout(string name)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _page.Layout = _nameManager.ResolveLayout(name, _page.Package);
            });
            return this;
        }

        IPageDefinition IPageDefinition.RegionComponent(string regionName, IComponent component)
        {
            _page.PopulateRegion(regionName, component);
            return this;
        }

        IPageDefinition IPageDefinition.RegionComponent(string regionName, string componentName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _page.PopulateRegion(regionName, _nameManager.ResolveComponent(componentName, _page.Package));
            });
            return this;
        }

        IPageDefinition IPageDefinition.RegionLayout(string regionName, ILayout layout)
        {
            _page.PopulateRegion(regionName, layout);
            return this;
        }

        IPageDefinition IPageDefinition.RegionLayout(string regionName, string layoutName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _page.PopulateRegion(regionName, _nameManager.ResolveLayout(layoutName, _page.Package));
            });
            return this;
        }

        IPageDefinition IPageDefinition.Title(string title)
        {
            if (string.IsNullOrEmpty(title))
                _page.TitleFunc = null;
            else
                _page.TitleFunc = (r, d) => title;
            return this;
        }

        IPageDefinition IPageDefinition.Title(Func<IRenderContext, string> titleFunc)
        {
            _page.TitleFunc = titleFunc;
            return this;
        }

        IPageDefinition IPageDefinition.BodyStyle(string cssStyle)
        {
            _page.BodyStyle = cssStyle;
            return this;
        }

        IPageDefinition IPageDefinition.BindTo<T>(string scopeName)
        {
            return this;
        }

        IPageDefinition IPageDefinition.BindTo(Type dataType, string scopeName)
        {
            return this;
        }

        IPageDefinition IPageDefinition.DataProvider(string providerName)
        {
            return this;
        }

        IPageDefinition IPageDefinition.DataProvider(IDataProvider dataProvider)
        {
            // TODO: Data binding
            return this;
        }

        IPageDefinition IPageDefinition.DataScope(string scopeName)
        {
            return this;
        }

        IPageDefinition IPageDefinition.DeployIn(IModule module)
        {
            _page.Module = module;
            return this;
        }

        IPageDefinition IPageDefinition.DeployIn(string moduleName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _page.Module = _nameManager.ResolveModule(moduleName);
            });
            return this;
        }

        IPageDefinition IPageDefinition.NeedsComponent(string componentName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _page.NeedsComponent(_nameManager.ResolveComponent(componentName, _page.Package));
            });
            return this;
        }

        IPageDefinition IPageDefinition.NeedsComponent(IComponent component)
        {
            _page.NeedsComponent(component);
            return this;
        }

        IPage IPageDefinition.Build()
        {
            _nameManager.Register(_page);
            return _page;
        }
    }
}
