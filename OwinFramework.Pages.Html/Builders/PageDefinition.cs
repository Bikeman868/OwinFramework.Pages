﻿using System;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class PageDefinition : IPageDefinition
    {
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IComponentDependenciesFactory _componentDependenciesFactory;
        private readonly Type _declaringType;
        private readonly Page _page;

        public PageDefinition(
            Page page,
            IRequestRouter requestRouter,
            INameManager nameManager,
            IFluentBuilder fluentBuilder,
            IComponentDependenciesFactory componentDependenciesFactory,
            IPackage package,
            Type declaringType)
        {
            _requestRouter = requestRouter;
            _nameManager = nameManager;
            _fluentBuilder = fluentBuilder;
            _componentDependenciesFactory = componentDependenciesFactory;
            _declaringType = declaringType;
            _page = page;

            if (package != null)
                _page.Package = package;
        }

        IPageDefinition IPageDefinition.Name(string name)
        {
            if (!string.IsNullOrEmpty(name))
                _page.Name = name;

            return this;
        }

        IPageDefinition IPageDefinition.CanonicalUrl(string canonicalUrl)
        {
            if (!string.IsNullOrEmpty(canonicalUrl))
                _page.CanonicalUrlFunc = rc => canonicalUrl;

            return this;
        }

        IPageDefinition IPageDefinition.CanonicalUrl(Func<IRenderContext, string> canonicalUrlFunc)
        {
            if (canonicalUrlFunc != null)
                _page.CanonicalUrlFunc = canonicalUrlFunc;

            return this;
        }

        IPageDefinition IPageDefinition.PartOf(IPackage package)
        {
            _page.Package = package;
            return this;
        }

        IPageDefinition IPageDefinition.PartOf(string packageName)
        {
            if (string.IsNullOrEmpty(packageName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolvePackageNames,
                (nm, p, n) => p.Package = nm.ResolvePackage(n),
                _page,
                packageName);

            return this;
        }

        IPageDefinition IPageDefinition.AssetDeployment(AssetDeployment assetDeployment)
        {
            _page.AssetDeployment = assetDeployment;
            return this;
        }

        IPageDefinition IPageDefinition.Route(string path, int priority, params Method[] methods)
        {
            if (methods == null || methods.Length == 0)
            {
                if (string.IsNullOrEmpty(path))
                    throw new PageBuilderException("The page route does not specify a path or any methods");

                _requestRouter.Register(_page, new FilterByPath(path), priority, _declaringType);
            }
            else
            {
                if (string.IsNullOrEmpty(path))
                {
                    _requestRouter.Register(_page, new FilterByMethod(methods), priority, _declaringType);
                }
                else
                {
                    _requestRouter.Register(
                        _page,
                        new FilterAllFilters(
                            new FilterByMethod(methods),
                            new FilterByPath(path)),
                        priority,
                        _declaringType);
                }
            }
            return this;
        }

        IPageDefinition IPageDefinition.Route(IRequestFilter filter, int priority)
        {
            _requestRouter.Register(_page, filter, priority, _declaringType);
            return this;
        }

        IPageDefinition IPageDefinition.RequiresPermission(string permissionName, string assetName)
        {
            if (string.IsNullOrEmpty(permissionName))
                return this;

            _page.AuthenticationFunc = c =>
                {
                    var authorization = c.GetFeature<IAuthorization>();
                    if (authorization == null)
                        throw new Exception(
                            "Page '" + _page.Name + "' requires permission '" + permissionName + 
                            "' but there is no Authorization middleware in the Owin pipeline");
                    return authorization.HasPermission(permissionName, assetName);
                };
            _page.AllowAnonymous = false;
            return this;
        }

        IPageDefinition IPageDefinition.RequiresIdentification()
        {
            _page.AllowAnonymous = false;
            return this;
        }

        IPageDefinition IPageDefinition.Cache(string cacheCategory, CachePriority cachePriority)
        {
            _page.CacheCategory = cacheCategory;
            _page.CachePriority = cachePriority;

            return this;
        }

        IPageDefinition IPageDefinition.Layout(ILayout layout)
        {
            _page.Layout = layout;
            return this;
        }

        IPageDefinition IPageDefinition.Layout(string layoutName)
        {
            if (string.IsNullOrEmpty(layoutName))
                throw new PageBuilderException("The name of the layout to use for the page is required");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, p, n) => { p.Layout = nm.ResolveLayout(n, p.Package); },
                _page,
                layoutName);
            return this;
        }

        IPageDefinition IPageDefinition.ZoneComponent(string zoneName, IComponent component)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a region name when configuring page regions");

            _page.PopulateRegion(zoneName, component);
            return this;
        }

        IPageDefinition IPageDefinition.ZoneComponent(string zoneName, string componentName)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a region name when configuring page regions");

            if (string.IsNullOrEmpty(componentName))
                throw new PageBuilderException("You must provide a component name when configuring a page region component");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences, 
                (nm, p, n) =>p.PopulateRegion(zoneName, nm.ResolveComponent(n, p.Package)),
                _page,
                componentName);
            return this;
        }

        IPageDefinition IPageDefinition.ZoneLayout(string zoneName, ILayout layout)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a region name when configuring page regions");

            _page.PopulateRegion(zoneName, layout);
            return this;
        }

        IPageDefinition IPageDefinition.ZoneLayout(string zoneName, string layoutName)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a region name when configuring a page region layout");

            if (string.IsNullOrEmpty(layoutName))
                throw new PageBuilderException("You must provide a layout name when configuring a page region layout");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, p, n) => p.PopulateRegion(zoneName, nm.ResolveLayout(n, p.Package)),
                _page,
                layoutName);
            return this;
        }

        IPageDefinition IPageDefinition.ZoneRegion(string zoneName, IRegion region)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a region name when configuring page regions");

            _page.PopulateRegion(zoneName, region);
            return this;
        }

        IPageDefinition IPageDefinition.ZoneRegion(string zoneName, string regionName)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a layout zone name when configuring a page zone layout");

            if (string.IsNullOrEmpty(regionName))
                throw new PageBuilderException("You must provide a region name when configuring a page region layout");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, p, n) => p.PopulateRegion(zoneName, nm.ResolveRegion(n, p.Package)),
                _page,
                regionName);
            return this;
        }

        IPageDefinition IPageDefinition.ZoneHtml(string zoneName, string textAssetName, string defaultHtml)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a region name when configuring page regions");

            var component = new HtmlComponent(_componentDependenciesFactory);
            component.Html(textAssetName, defaultHtml);

            _page.PopulateRegion(zoneName, component);

            return this;
        }

        IPageDefinition IPageDefinition.ZoneTemplate(string zoneName, string templatePath)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a region name when configuring page regions");

            var component = new TemplateComponent(_componentDependenciesFactory);
            component.BodyTemplate(templatePath);

            _page.PopulateRegion(zoneName, component);

            return this;
        }

        IPageDefinition IPageDefinition.Title(string title)
        {
            if (string.IsNullOrEmpty(title))
                _page.TitleFunc = null;
            else
                _page.TitleFunc = r => title;
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
            var dataConsumer = _page as IDataConsumer;
            if (dataConsumer == null)
                throw new PageBuilderException("This page is not a consumer of data");

            dataConsumer.HasDependency<T>(scopeName);

            return this;
        }

        IPageDefinition IPageDefinition.BindTo(Type dataType, string scopeName)
        {
            if (dataType == null)
                throw new PageBuilderException("To define data binding you must specify the type of data to bind");

            var dataConsumer = _page as IDataConsumer;
            if (dataConsumer == null)
                throw new PageBuilderException("This layout is not a consumer of data");

            dataConsumer.HasDependency(dataType, scopeName);

            return this;
        }

        IPageDefinition IPageDefinition.DataProvider(string dataProviderName)
        {
            var dataConsumer = _page as IDataConsumer;
            if (dataConsumer == null)
                throw new PageBuilderException("This page is not a consumer of data");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, c, n) => c.HasDependency(nm.ResolveDataProvider(n)),
                dataConsumer,
                dataProviderName);

            return this;
        }

        IPageDefinition IPageDefinition.DataProvider(IDataProvider dataProvider)
        {
            var dataConsumer = _page as IDataConsumer;
            if (dataConsumer == null)
                throw new PageBuilderException("This page is not a consumer of data");

            dataConsumer.HasDependency(dataProvider);

            return this;
        }

        IPageDefinition IPageDefinition.DataScope(Type type, string scopeName)
        {
            if (type == null)
                throw new PageBuilderException("The page data scope type is null");

            var dataScope = _page as IDataScopeRules;
            if (dataScope == null)
                throw new PageBuilderException("This page is not a data scope provider");

            dataScope.AddScope(type, scopeName);

            return this;
        }

        IPageDefinition IPageDefinition.DeployIn(IModule module)
        {
            _page.Module = module;
            return this;
        }

        IPageDefinition IPageDefinition.DeployIn(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, p, n) => p.Module = nm.ResolveModule(n),
                _page,
                moduleName);

            return this;
        }

        IPageDefinition IPageDefinition.NeedsComponent(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
                throw new PageBuilderException("No component name provided in page dependency");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, p, n) => p.NeedsComponent(nm.ResolveComponent(n, p.Package)),
                _page,
                componentName);

            return this;
        }

        IPageDefinition IPageDefinition.NeedsComponent(IComponent component)
        {
            if (ReferenceEquals(component, null))
                throw new PageBuilderException("Null component reference for dependent component");

            _page.NeedsComponent(component);

            return this;
        }

        IPage IPageDefinition.Build()
        {
            _fluentBuilder.Register(_page);
            _nameManager.AddResolutionHandler(NameResolutionPhase.InitializeRunables, () => _page.Initialize());
            return _page;
        }
    }
}
