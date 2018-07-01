using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;

namespace OwinFramework.Pages.Framework.Builders
{
    /// <summary>
    /// Uses reflection to find pages, layouts, regions, components
    /// and services in the solution and registers then and wires
    /// them together
    /// </summary>
    internal class FluentBuilder : IFluentBuilder
    {
        public IModuleBuilder ModuleBuilder { get; set; }
        public IPageBuilder PageBuilder { get; set; }
        public ILayoutBuilder LayoutBuilder { get; set; }
        public IRegionBuilder RegionBuilder { get; set; }
        public IComponentBuilder ComponentBuilder { get; set; }
        public IServiceBuilder ServiceBuilder { get; set; }

        private readonly INameManager _nameManager;
        private readonly IRequestRouter _requestRouter;
        private readonly HashSet<string> _assemblies;
        private readonly HashSet<Type> _types;
        private readonly IPackage _packageContext;

        public FluentBuilder(
            INameManager nameManager,
            IRequestRouter requestRouter)
        {
            _nameManager = nameManager;
            _requestRouter = requestRouter;
            _assemblies = new HashSet<string>();
            _types = new HashSet<Type>();
        }

        private FluentBuilder(
            FluentBuilder parent,
            IPackage packageContext)
        {
            _nameManager = parent._nameManager;
            _assemblies = parent._assemblies;
            _types = parent._types;
            _packageContext = packageContext;

            ModuleBuilder = parent.ModuleBuilder;
            PageBuilder = parent.PageBuilder;
            LayoutBuilder = parent.LayoutBuilder;
            RegionBuilder = parent.RegionBuilder;
            ComponentBuilder = parent.ComponentBuilder;
            ServiceBuilder = parent.ServiceBuilder;
        }

        IPackage IFluentBuilder.Register(IPackage package, string namespaceName)
        {
            var type = package.GetType();
            if (!_types.Add(type)) return package;

            var attributes = new AttributeSet(type);

            if (!string.IsNullOrEmpty(namespaceName) && attributes.IsPackage != null)
            {
                    attributes.IsPackage.NamespaceName = namespaceName;
            }

            return BuildPackage(attributes, package, null);
        }

        void IFluentBuilder.Register(Assembly assembly, Func<Type, object> factory)
        {
            if (!_assemblies.Add(assembly.FullName))
                return;

            var types = assembly.GetTypes();

            var packageTypes = types.Where(t => t.GetCustomAttributes(true).Any(a => a is IsPackageAttribute)).ToList();
            var otherTypes = types.Where(t => !t.GetCustomAttributes(true).Any(a => a is IsPackageAttribute)).ToList();

            var  exceptions = new List<Exception>();

            // Must register packages first because they define the 
            // namespace for the other elements
            foreach (var type in packageTypes)
            {
                try
                {
                    ((IFluentBuilder)this).Register(type, factory);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            // Register everything else
            foreach (var type in otherTypes)
            {
                try
                {
                    ((IFluentBuilder)this).Register(type, factory);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count == 1)
                throw exceptions[0];

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);
        }

        object IFluentBuilder.Register(Type type, Func<Type, object> factory)
        {
            if (!_types.Add(type))
                return null;

            var attributes = new AttributeSet(type);

            if (attributes.IsPackage != null) return BuildPackage(attributes, null, factory);
            if (attributes.IsModule != null) return BuildModule(attributes, null, factory);
            if (attributes.IsPage != null) return BuildPage(attributes, null, factory);
            if (attributes.IsLayout != null) return BuildLayout(attributes, null, factory);
            if (attributes.IsRegion != null) return BuildRegion(attributes, null, factory);
            if (attributes.IsComponent != null) return BuildComponent(attributes, null, factory);
            if (attributes.IsService != null) return BuildService(attributes, null, factory);
            if (attributes.IsDataProvider != null) return BuildDataProvider(attributes, null, factory);

            return null;
        }

        T IFluentBuilder.Register<T>(T element)
        {
            var type = element.GetType();
            if (!_types.Add(type)) return element;

            var attributes = new AttributeSet(type);

            var package = element as IPackage;
            if (package != null) BuildPackage(attributes, package, null);

            var module = element as IModule;
            if (module != null) BuildModule(attributes, module, null);

            var page = element as IPage;
            if (page != null) BuildPage(attributes, page, null);

            var layout = element as ILayout;
            if (layout != null) BuildLayout(attributes, layout, null);

            var Region = element as IRegion;
            if (Region != null) BuildRegion(attributes, Region, null);

            var component = element as IComponent;
            if (component != null) BuildComponent(attributes, component, null);

            var service = element as IService;
            if (service != null) BuildService(attributes, service, null);

            var dataProvider = element as IDataProvider;
            if (dataProvider != null) BuildDataProvider(attributes, dataProvider, null);

            return element;
        }

        public IComponentDefinition Component(IPackage package)
        {
            if (ComponentBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build components");
            return ComponentBuilder.Component(package ?? _packageContext);
        }

        public IRegionDefinition Region(IPackage package)
        {
            if (RegionBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build regions");
            return RegionBuilder.Region(package ?? _packageContext);
        }

        public ILayoutDefinition Layout(IPackage package)
        {
            if (LayoutBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build layouts");
            return LayoutBuilder.Layout(package ?? _packageContext);
        }

        public IPageDefinition Page(Type declaringType, IPackage package)
        {
            if (PageBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build pages");
            return PageBuilder.Page(declaringType, package ?? _packageContext);
        }

        public IServiceDefinition Service(Type declaringType, IPackage package)
        {
            if (ServiceBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build services");
            return ServiceBuilder.Service(declaringType, package ?? _packageContext);
        }

        public IModuleDefinition Module()
        {
            if (ModuleBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build modules");
            return ModuleBuilder.Module();
        }

        private IPackage BuildPackage(AttributeSet attributes, IPackage package, Func<Type, object> factory)
        {
            if (package == null && factory != null && typeof(IPackage).IsAssignableFrom(attributes.Type))
                package = factory(attributes.Type) as IPackage;

            if (package != null)
            {
                if (attributes.IsPackage != null)
                {
                    if (string.IsNullOrEmpty(package.Name))
                        package.Name = attributes.IsPackage.Name;

                    if (string.IsNullOrEmpty(package.NamespaceName))
                        package.NamespaceName = attributes.IsPackage.NamespaceName;
                }

                package.Build(new FluentBuilder(this, package));

                _nameManager.Register(package);
                return package;
            }

            IPackageDefinition packageDefinition = new PackageDefinition(attributes.Type, this, _nameManager);
            
            packageDefinition.Name(attributes.IsPackage.Name);
            packageDefinition.NamespaceName(attributes.IsPackage.NamespaceName);

            if (attributes.DeployedAs != null)
                packageDefinition.Module(attributes.DeployedAs.ModuleName);

            return packageDefinition.Build();
        }

        private IModule BuildModule(AttributeSet attributes, IModule module, Func<Type, object> factory)
        {
            if (module == null && factory != null && typeof(IModule).IsAssignableFrom(attributes.Type))
                module = factory(attributes.Type) as IModule;

            if (module != null)
            {
                if (attributes.IsModule != null)
                {
                    if (!string.IsNullOrEmpty(attributes.IsModule.Name))
                        module.Name = attributes.IsModule.Name;
                    module.AssetDeployment = attributes.IsModule.AssetDeployment;
                }
                _nameManager.Register(module);
                return module;
            }

            var moduleDefinition = Module()
                .Name(attributes.IsModule.Name)
                .AssetDeployment(attributes.IsModule.AssetDeployment);

            return moduleDefinition.Build();
        }

        private IPage BuildPage(AttributeSet attributes, IPage page, Func<Type, object> factory)
        {
            if (page == null && factory != null && typeof(IPage).IsAssignableFrom(attributes.Type))
                page = factory(attributes.Type) as IPage;

            if (page != null)
            {
                Configure(attributes, page);
                return page;
            }

            var pageDefinition = Page(attributes.Type, _packageContext)
                .Name(attributes.IsPage.Name);

            if (attributes.DeployedAs != null)
                pageDefinition.AssetDeployment(attributes.DeployedAs.Deployment)
                    .Module(attributes.DeployedAs.ModuleName);

            if (attributes.UsesLayouts != null)
                foreach(var usesLayout in attributes.UsesLayouts)
                    pageDefinition.Layout(usesLayout.LayoutName);

            if (attributes.PartOf != null)
                pageDefinition.PartOf(attributes.PartOf.PackageName);

            if (attributes.PageTitle != null)
                pageDefinition.Title(attributes.PageTitle.Title);

            if (attributes.Style != null)
                pageDefinition.BodyStyle(attributes.Style.CssStyle);

            if (attributes.NeedsDatas != null)
            {
                foreach (var needsData in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(needsData.DataProviderName))
                        pageDefinition.DataProvider(needsData.DataProviderName);

                    if (needsData.DataType != null)
                        pageDefinition.BindTo(needsData.DataType);
                }
            }

            if (attributes.RegionComponents != null)
            {
                foreach (var regionComponent in attributes.RegionComponents)
                {
                    pageDefinition.RegionComponent(regionComponent.Region, regionComponent.Component);
                }
            }

            if (attributes.RegionLayouts != null)
            {
                foreach (var regionLayout in attributes.RegionLayouts)
                {
                    pageDefinition.RegionLayout(regionLayout.Region, regionLayout.Layout);
                }
            }

            if (attributes.Routes != null)
            {
                foreach(var route in attributes.Routes)
                {
                    pageDefinition.Route(route.Path, route.Priority, route.Methods);
                }
            }

            if (attributes.NeedsComponents != null)
            {
                foreach (var component in attributes.NeedsComponents)
                {
                    pageDefinition.NeedsComponent(component.ComponentName);
                }
            }

            return pageDefinition.Build();
        }

        private ILayout BuildLayout(AttributeSet attributes, ILayout layout, Func<Type, object> factory)
        {
            if (layout == null && factory != null && typeof(ILayout).IsAssignableFrom(attributes.Type))
                layout = factory(attributes.Type) as ILayout;

            if (layout != null)
            {
                Configure(attributes, layout);
                _nameManager.Register(layout);
                return layout;
            }

            var layoutDefinition = Layout(_packageContext)
                .Name(attributes.IsLayout.Name)
                .RegionNesting(attributes.IsLayout.RegionNesting);

            if (attributes.DeployedAs != null)
                layoutDefinition.AssetDeployment(attributes.DeployedAs.Deployment)
                    .DeployIn(attributes.DeployedAs.ModuleName);

            if (attributes.PartOf != null)
                layoutDefinition.PartOf(attributes.PartOf.PackageName);

            if (attributes.NeedsDatas != null)
            {
                foreach (var needsData in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(needsData.DataProviderName))
                        layoutDefinition.DataProvider(needsData.DataProviderName);

                    if (needsData.DataType != null)
                        layoutDefinition.BindTo(needsData.DataType);
                }
            }

            if (attributes.RegionComponents != null)
                foreach(var regionComponent in attributes.RegionComponents)
                    layoutDefinition.Component(regionComponent.Region, regionComponent.Component);

            if (attributes.RegionLayouts != null)
                foreach (var regionLayout in attributes.RegionLayouts)
                    layoutDefinition.Layout(regionLayout.Region, regionLayout.Layout);

            if (attributes.Style != null)
            {
                if (!string.IsNullOrEmpty(attributes.Style.CssStyle))
                    layoutDefinition.Style(attributes.Style.CssStyle);
            }

            if (attributes.ChildStyle != null)
            {
                if (!string.IsNullOrEmpty(attributes.ChildStyle.CssStyle))
                    layoutDefinition.NestedStyle(attributes.ChildStyle.CssStyle);
            }

            if (attributes.Container != null)
            {
                if (!string.IsNullOrEmpty(attributes.Container.Tag))
                    layoutDefinition.Tag(attributes.Container.Tag);

                if (attributes.Container.ClassNames != null && attributes.Container.ClassNames.Length > 0)
                    layoutDefinition.ClassNames(attributes.Container.ClassNames);
            }

            if (attributes.NeedsComponents != null)
            {
                foreach (var component in attributes.NeedsComponents)
                {
                    layoutDefinition.NeedsComponent(component.ComponentName);
                }
            }

            if (attributes.ChildContainer != null)
            {
                if (!string.IsNullOrEmpty(attributes.ChildContainer.Tag))
                    layoutDefinition.NestingTag(attributes.ChildContainer.Tag);

                if (attributes.ChildContainer.ClassNames != null && attributes.ChildContainer.ClassNames.Length > 0)
                    layoutDefinition.NestedClassNames(attributes.ChildContainer.ClassNames);
            }

            if (attributes.UsesRegions != null)
                foreach (var usesRegion in attributes.UsesRegions)
                    layoutDefinition.Region(usesRegion.RegionName, usesRegion.RegionElement);
    
            return layoutDefinition.Build();
        }

        private IRegion BuildRegion(AttributeSet attributes, IRegion region, Func<Type, object> factory)
        {
            if (region == null && factory != null && typeof(IRegion).IsAssignableFrom(attributes.Type))
                region = factory(attributes.Type) as IRegion;

            if (region != null)
            {
                Configure(attributes, region);
                _nameManager.Register(region);
                return region;
            }

            var regionDefinition = Region(_packageContext)
                .Name(attributes.IsRegion.Name);

            if (attributes.DeployedAs != null)
                regionDefinition.AssetDeployment(attributes.DeployedAs.Deployment)
                    .DeployIn(attributes.DeployedAs.ModuleName);

            if (attributes.NeedsDatas != null)
            {
                foreach (var needsData in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(needsData.DataProviderName))
                        regionDefinition.DataProvider(needsData.DataProviderName);

                    if (needsData.DataType != null)
                        regionDefinition.BindTo(needsData.DataType);
                }
            }

            if (attributes.PartOf != null)
                regionDefinition.PartOf(attributes.PartOf.PackageName);

            if (attributes.Style != null)
            {
                if (!string.IsNullOrEmpty(attributes.Style.CssStyle))
                    regionDefinition.Style(attributes.Style.CssStyle);
            }

            if (attributes.Repeat != null)
                regionDefinition.ForEach(
                    attributes.Repeat.ItemType, 
                    attributes.Repeat.Tag, 
                    attributes.Repeat.Style, 
                    attributes.Repeat.ClassNames);

            if (attributes.Container != null)
            {
                if (!string.IsNullOrEmpty(attributes.Container.Tag))
                    regionDefinition.Tag(attributes.Container.Tag);

                if (attributes.Container.ClassNames != null && attributes.Container.ClassNames.Length > 0)
                    regionDefinition.ClassNames(attributes.Container.ClassNames);
            }

            if (attributes.NeedsComponents != null)
            {
                foreach (var component in attributes.NeedsComponents)
                {
                    regionDefinition.NeedsComponent(component.ComponentName);
                }
            }

            if (attributes.UsesLayouts != null)
                foreach(var usesLayout in attributes.UsesLayouts)
                    regionDefinition.Layout(usesLayout.LayoutName);

            if (attributes.UsesComponents != null)
                foreach(var usesComponent in attributes.UsesComponents)
                    regionDefinition.Component(usesComponent.ComponentName);

            return regionDefinition.Build();
        }

        private IComponent BuildComponent(AttributeSet attributes, IComponent component, Func<Type, object> factory)
        {
            if (component == null && factory != null && typeof(IComponent).IsAssignableFrom(attributes.Type))
                component = factory(attributes.Type) as IComponent;

            if (component != null)
            {
                Configure(attributes, component);
                _nameManager.Register(component);
                return component;
            }

            var componentDefinition = Component(_packageContext)
            .Name(attributes.IsComponent.Name);

            if (attributes.DeployedAs != null)
                componentDefinition.AssetDeployment(attributes.DeployedAs.Deployment)
                    .DeployIn(attributes.DeployedAs.ModuleName);

            if (attributes.PartOf != null)
                componentDefinition.PartOf(attributes.PartOf.PackageName);

            if (attributes.NeedsDatas != null)
            {
                foreach (var needsData in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(needsData.DataProviderName))
                        componentDefinition.DataProvider(needsData.DataProviderName);

                    if (needsData.DataType != null)
                        componentDefinition.BindTo(needsData.DataType);
                }
            }

            if (attributes.RenderHtmls != null)
            {
                foreach(var renderHtml in attributes.RenderHtmls.OrderBy(r => r.Order))
                {
                    componentDefinition.Render(renderHtml.TextName, renderHtml.Html);
                }
            }

            if (attributes.NeedsComponents != null)
            {
                foreach (var neededComponent in attributes.NeedsComponents)
                {
                    componentDefinition.NeedsComponent(neededComponent.ComponentName);
                }
            }

            if (attributes.DeployCsss != null)
                foreach(var deployCss in attributes.DeployCsss)
                    componentDefinition.DeployCss(deployCss.CssSelector, deployCss.CssStyle);

            if (attributes.DeployFunction != null)
                componentDefinition.DeployFunction(
                    attributes.DeployFunction.ReturnType,
                    attributes.DeployFunction.FunctionName,
                    attributes.DeployFunction.Parameters,
                    attributes.DeployFunction.Body,
                    attributes.DeployFunction.IsPublic);

            return componentDefinition.Build();
        }

        private IService BuildService(AttributeSet attributes, IService service, Func<Type, object> factory)
        {
            if (service == null && factory != null && typeof(IService).IsAssignableFrom(attributes.Type))
                service = factory(attributes.Type) as IService;

            if (service != null)
            {
                Configure(attributes, service);
                return service;
            }

            var serviceDefinition = Service(attributes.Type, _packageContext)
                .Name(attributes.IsService.Name);

            return serviceDefinition.Build();
        }

        private IDataProvider BuildDataProvider(AttributeSet attributes, IDataProvider dataProvider, Func<Type, object> factory)
        {
            return dataProvider;
        }

        private void Configure(AttributeSet attributes, IPage page)
        {
            if (attributes.IsPage != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsPage.Name))
                    page.Name = attributes.IsPage.Name;
            }

            if (attributes.PartOf != null && !string.IsNullOrEmpty(attributes.PartOf.PackageName))
            {
                page.Package = _nameManager.ResolvePackage(attributes.PartOf.PackageName);
            }

            ConfigureRunable(attributes, page);
        }

        private void Configure(AttributeSet attributes, ILayout layout)
        {
            if (attributes.IsLayout != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsLayout.Name))
                    layout.Name = attributes.IsLayout.Name;
            }

            if (attributes.RegionComponents != null)
            {
                foreach (var regionComponent in attributes.RegionComponents)
                {
                    var rc = regionComponent;
                    _nameManager.AddResolutionHandler(() =>
                    {
                        layout.Populate(rc.Region, _nameManager.ResolveComponent(rc.Component, layout.Package));
                    });
                }
            }

            if (attributes.RegionLayouts != null)
            {
                foreach (var regionLayout in attributes.RegionLayouts)
                {
                    var rl = regionLayout;
                    _nameManager.AddResolutionHandler(() =>
                    {
                        layout.Populate(rl.Region, _nameManager.ResolveLayout(rl.Layout, layout.Package));
                    });
                }
            }

            ConfigureElement(attributes, layout);
        }

        private void Configure(AttributeSet attributes, IRegion region)
        {
            if (attributes.IsRegion != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsRegion.Name))
                    region.Name = attributes.IsRegion.Name;
            }

            if (attributes.UsesComponents != null)
            {
                if (attributes.UsesComponents.Count > 1)
                    throw new RegionBuilderException("Regions can only host one component but you " +
                        "have more than one [UsesComponent] attribute on " + attributes.Type.DisplayName());
                if (attributes.UsesComponents.Count == 1)
                {
                    _nameManager.AddResolutionHandler(() =>
                    {
                        region.Populate(_nameManager.ResolveComponent(attributes.UsesComponents[0].ComponentName, region.Package));
                    });
                }
            }

            if (attributes.UsesLayouts != null)
            {
                if (attributes.UsesLayouts.Count > 1)
                    throw new RegionBuilderException("Regions can only host one layout but you " +
                        "have more than one [UsesLayout] attribute on " + attributes.Type.DisplayName());
                if (attributes.UsesLayouts.Count == 1)
                {
                    _nameManager.AddResolutionHandler(() =>
                    {
                        region.Populate(_nameManager.ResolveLayout(attributes.UsesLayouts[0].LayoutName, region.Package));
                    });
                }
            }

            ConfigureElement(attributes, region);
        }

        private void Configure(AttributeSet attributes, IComponent component)
        {
            if (attributes.IsComponent != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsComponent.Name))
                    component.Name = attributes.IsComponent.Name;
            }
            ConfigureElement(attributes, component);
        }

        private void Configure(AttributeSet attributes, IService service)
        {
            if (attributes.IsService != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsService.Name))
                    service.Name = attributes.IsService.Name;
            }

            if (attributes.PartOf != null && !string.IsNullOrEmpty(attributes.PartOf.PackageName))
            {
                service.Package = _nameManager.ResolvePackage(attributes.PartOf.PackageName);
            }

            ConfigureRunable(attributes, service);
        }

        private void ConfigureRunable(AttributeSet attributes, IRunable runable)
        {
            if (attributes.RequiresPermission != null)
            {
                runable.RequiredPermission = attributes.RequiresPermission.PermissionName;
            }

            if (attributes.Routes != null)
            {
                foreach (var route in attributes.Routes)
                {
                    if (route.Methods == null || route.Methods.Length == 0)
                    {
                        if (string.IsNullOrEmpty(route.Path))
                            continue;
                        _requestRouter.Register(runable, new FilterByPath(route.Path), route.Priority);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(route.Path))
                            _requestRouter.Register(runable, new FilterByMethod(route.Methods), route.Priority);
                        else
                            _requestRouter.Register(
                                runable,
                                new FilterAllFilters(
                                    new FilterByMethod(route.Methods),
                                    new FilterByPath(route.Path)),
                                route.Priority);
                    }
                }
            }
        }

        private void ConfigureElement(AttributeSet attributes, IElement element)
        {
            if (attributes.PartOf != null && !string.IsNullOrEmpty(attributes.PartOf.PackageName))
            {
                element.Package = _nameManager.ResolvePackage(attributes.PartOf.PackageName);
            }

            if (attributes.DeployedAs != null)
            {
                element.AssetDeployment = attributes.DeployedAs.Deployment;
                if (!string.IsNullOrEmpty(attributes.DeployedAs.ModuleName))
                {
                    _nameManager.AddResolutionHandler(() =>
                    {
                        element.Module = _nameManager.ResolveModule(attributes.DeployedAs.ModuleName);
                    });
                }
            }

            var dataConsumer = element as IDataConsumer;
            if (dataConsumer != null)
            {
                if (attributes.NeedsDatas != null)
                {
                    foreach (var need in attributes.NeedsDatas)
                    {
                        if (need.DataType != null || !string.IsNullOrEmpty(need.Scope))
                            dataConsumer.NeedsData(need.DataType, need.Scope);

                        if (!string.IsNullOrEmpty(need.DataProviderName))
                            _nameManager.AddResolutionHandler(
                                (nm, dc) => dc.NeedsProvider(nm.ResolveDataProvider(need.DataProviderName)), 
                                dataConsumer);
                    }
                }
            }

            var dataScopeProvider = element as IDataScopeProvider;
            if (dataScopeProvider != null)
            {
                if (attributes.DataScopes != null)
                {
                    foreach(var dataScope in attributes.DataScopes)
                    {
                        dataScopeProvider.AddScope(dataScope.DataType, dataScope.Scope);
                    }
                }

                if (attributes.Repeat != null)
                {
                    dataScopeProvider.AddScope(attributes.Repeat.ItemType, attributes.Repeat.ScopeName);
                }
            }
        }
    }
}
