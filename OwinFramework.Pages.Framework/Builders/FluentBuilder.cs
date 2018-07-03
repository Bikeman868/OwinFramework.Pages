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

        #region Register a package overriding the namespace

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

        #endregion

        #region Enumerating types in an assembly

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

        #endregion

        #region Registering types that use attributes to define elements declaratively

        object IFluentBuilder.Register(Type type, Func<Type, object> factory)
        {
            if (!_types.Add(type)) return null;

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

        #endregion

        #region Registering pre-built elements

        T IFluentBuilder.Register<T>(T element, Type type)
        {
            if (element == null) return null;
            type = type ?? element.GetType();

            var attributes = new AttributeSet(type);

            var package = element as IPackage;
            if (package != null)
                return (T)BuildPackage(attributes, package, null);

            var module = element as IModule;
            if (module != null)
                return (T)BuildModule(attributes, module, null);

            var page = element as IPage;
            if (page != null) 
                return (T)BuildPage(attributes, page, null);

            var layout = element as ILayout;
            if (layout != null) 
                return (T)BuildLayout(attributes, layout, null);

            var region = element as IRegion;
            if (region != null) 
                return (T)BuildRegion(attributes, region, null);

            var component = element as IComponent;
            if (component != null) 
                return (T)BuildComponent(attributes, component, null);

            var service = element as IService;
            if (service != null) 
                return (T)BuildService(attributes, service, null);

            var dataProvider = element as IDataProvider;
            if (dataProvider != null) 
                return (T)BuildDataProvider(attributes, dataProvider, null);

            throw new FluentBuilderException("Fluent builder does no know how to register instance of " + type.DisplayName());
        }

        #endregion

        #region Entry points for fluently building various types of element

        public IComponentDefinition Component(Type declaringType, IPackage package)
        {
            if (ComponentBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build components");
            return ComponentBuilder.Component(declaringType, package ?? _packageContext);
        }

        public IRegionDefinition Region(Type declaringType, IPackage package)
        {
            if (RegionBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build regions");
            return RegionBuilder.Region(declaringType, package ?? _packageContext);
        }

        public ILayoutDefinition Layout(Type declaringType, IPackage package)
        {
            if (LayoutBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build layouts");
            return LayoutBuilder.Layout(declaringType, package ?? _packageContext);
        }

        public IPageDefinition Page(Type declaringType, IPackage package)
        {
            if (PageBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build pages");
            return PageBuilder.Page(declaringType, package ?? _packageContext);
        }

        public IServiceDefinition Service(Type declaringType, IPackage package)
        {
            if (ServiceBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build services");
            return ServiceBuilder.Service(declaringType, package ?? _packageContext);
        }

        public IModuleDefinition Module(Type declaringType)
        {
            if (ModuleBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build modules");
            return ModuleBuilder.Module(declaringType);
        }

        #endregion

        #region Builders for each element type

        private IPackage BuildPackage(AttributeSet attributes, IPackage package, Func<Type, object> factory)
        {
            if (package == null && factory != null && typeof(IPackage).IsAssignableFrom(attributes.Type))
                package = factory(attributes.Type) as IPackage;

            if (package == null)
                return ((IPackageDefinition)new PackageDefinition(attributes.Type, this, _nameManager)).Build();

            Configure(attributes, package);
            _nameManager.Register(package);

            package.Build(new FluentBuilder(this, package));
            return package;
        }

        private IModule BuildModule(AttributeSet attributes, IModule module, Func<Type, object> factory)
        {
            if (module == null && factory != null && typeof(IModule).IsAssignableFrom(attributes.Type))
                module = factory(attributes.Type) as IModule;

            if (module == null)
                return Module(attributes.Type).Build();

            Configure(attributes, module);
            _nameManager.Register(module);

            return module;
        }

        private IPage BuildPage(AttributeSet attributes, IPage page, Func<Type, object> factory)
        {
            if (page == null && factory != null && typeof(IPage).IsAssignableFrom(attributes.Type))
                page = factory(attributes.Type) as IPage;

            if (page == null)
            {
                var pageDefinition = Page(attributes.Type, _packageContext);

                if (attributes.UsesLayouts != null)
                    foreach (var usesLayout in attributes.UsesLayouts)
                        pageDefinition.Layout(usesLayout.LayoutName);

                if (attributes.PageTitle != null)
                    pageDefinition.Title(attributes.PageTitle.Title);

                if (attributes.Style != null)
                    pageDefinition.BodyStyle(attributes.Style.CssStyle);

                if (attributes.RegionComponents != null)
                {
                    foreach (var regionComponent in attributes.RegionComponents)
                        pageDefinition.RegionComponent(regionComponent.Region, regionComponent.Component);
                }

                if (attributes.RegionLayouts != null)
                {
                    foreach (var regionLayout in attributes.RegionLayouts)
                        pageDefinition.RegionLayout(regionLayout.Region, regionLayout.Layout);
                }

                if (attributes.NeedsComponents != null)
                {
                    foreach (var component in attributes.NeedsComponents)
                        pageDefinition.NeedsComponent(component.ComponentName);
                }

                return pageDefinition.Build();
            }

            Configure(attributes, page);
            _nameManager.Register(page);

            return page;
        }

        private ILayout BuildLayout(AttributeSet attributes, ILayout layout, Func<Type, object> factory)
        {
            if (layout == null && factory != null && typeof(ILayout).IsAssignableFrom(attributes.Type))
                layout = factory(attributes.Type) as ILayout;

            if (layout == null)
            {
                var layoutDefinition = Layout(attributes.Type, _packageContext)
                    .RegionNesting(attributes.IsLayout.RegionNesting);

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

            Configure(attributes, layout);
            _nameManager.Register(layout);
            return layout;
        }

        private IRegion BuildRegion(AttributeSet attributes, IRegion region, Func<Type, object> factory)
        {
            if (region == null && factory != null && typeof(IRegion).IsAssignableFrom(attributes.Type))
                region = factory(attributes.Type) as IRegion;

            if (region == null)
            {
                var regionDefinition = Region(attributes.Type, _packageContext);

                if (attributes.Style != null)
                {
                    if (!string.IsNullOrEmpty(attributes.Style.CssStyle))
                        regionDefinition.Style(attributes.Style.CssStyle);
                }

                if (attributes.Repeat != null)
                    regionDefinition.ForEach(
                        attributes.Repeat.ItemType, 
                        attributes.Repeat.ScopeName,
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
                        regionDefinition.NeedsComponent(component.ComponentName);
                }

                if (attributes.UsesLayouts != null)
                    foreach(var usesLayout in attributes.UsesLayouts)
                        regionDefinition.Layout(usesLayout.LayoutName);

                if (attributes.UsesComponents != null)
                    foreach(var usesComponent in attributes.UsesComponents)
                        regionDefinition.Component(usesComponent.ComponentName);

                return regionDefinition.Build();
            }

            Configure(attributes, region);
            _nameManager.Register(region);

            return region;
        }

        private IComponent BuildComponent(AttributeSet attributes, IComponent component, Func<Type, object> factory)
        {
            if (component == null && factory != null && typeof(IComponent).IsAssignableFrom(attributes.Type))
                component = factory(attributes.Type) as IComponent;

            if (component == null)
            {
                var componentDefinition = Component(attributes.Type, _packageContext);

                if (attributes.RenderHtmls != null)
                {
                    foreach(var renderHtml in attributes.RenderHtmls.OrderBy(r => r.Order))
                        componentDefinition.Render(renderHtml.TextName, renderHtml.Html);
                }

                if (attributes.NeedsComponents != null)
                {
                    foreach (var neededComponent in attributes.NeedsComponents)
                        componentDefinition.NeedsComponent(neededComponent.ComponentName);
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

            Configure(attributes, component);
            _nameManager.Register(component);

            return component;
        }

        private IService BuildService(AttributeSet attributes, IService service, Func<Type, object> factory)
        {
            if (service == null && factory != null && typeof(IService).IsAssignableFrom(attributes.Type))
                service = factory(attributes.Type) as IService;

            if (service == null)
                return Service(attributes.Type, _packageContext).Build();

            Configure(attributes, service);
            _nameManager.Register(service);

            return service;
        }

        private IDataProvider BuildDataProvider(AttributeSet attributes, IDataProvider dataProvider, Func<Type, object> factory)
        {
            if (dataProvider == null && factory != null && typeof(IDataProvider).IsAssignableFrom(attributes.Type))
                dataProvider = factory(attributes.Type) as IDataProvider;

            //if (dataProvider == null)
            //{
            //    var dataProviderDefinition = DataProvider(attributes.Type, _packageContext).Build();
            //    return dataProviderDefinition.Build(attributes.Type);
            //}

            //Configure(attributes, dataProvider);
            return dataProvider;
        }

        #endregion

        #region Configure instances based on attributes attached to declaring type

        private void Configure(AttributeSet attributes, IPackage package)
        {
            if (attributes.IsPackage != null)
            {
                if (string.IsNullOrEmpty(package.Name))
                    package.Name = attributes.IsPackage.Name;

                if (string.IsNullOrEmpty(package.NamespaceName))
                    package.NamespaceName = attributes.IsPackage.NamespaceName;
            }

            if (attributes.DeployedAs != null && !string.IsNullOrEmpty(attributes.DeployedAs.ModuleName))
                _nameManager.AddResolutionHandler((nm, p) => p.Module = nm.ResolveModule(attributes.DeployedAs.ModuleName), package);
        }

        private void Configure(AttributeSet attributes, IModule module)
        {
            if (module == null) return;

            if (attributes.IsModule != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsModule.Name))
                    module.Name = attributes.IsModule.Name;
                module.AssetDeployment = attributes.IsModule.AssetDeployment;
            }
        }

        private void Configure(AttributeSet attributes, IPage page)
        {
            if (page == null) return;

            if (attributes.IsPage != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsPage.Name))
                    page.Name = attributes.IsPage.Name;
            }

            if (attributes.PartOf != null && !string.IsNullOrEmpty(attributes.PartOf.PackageName))
            {
                page.Package = _nameManager.ResolvePackage(attributes.PartOf.PackageName);
            }

            Configure(attributes, page as IRunable);
            Configure(attributes, page as IDataConsumer);
            Configure(attributes, page as IDataScopeProvider);
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

            Configure(attributes, layout as IElement);
            Configure(attributes, layout as IDataConsumer);
        }

        private void Configure(AttributeSet attributes, IRegion region)
        {
            if (attributes.IsRegion != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsRegion.Name))
                    region.Name = attributes.IsRegion.Name;
            }

            if (attributes.Repeat != null)
            {
                region.RepeatScope = attributes.Repeat.ScopeName;
                region.RepeatType = attributes.Repeat.ItemType;
                region.RepeatType = attributes.Repeat.ItemType;
                region.RepeatType = attributes.Repeat.ItemType;
                region.RepeatType = attributes.Repeat.ItemType;
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

            Configure(attributes, region as IElement);
            Configure(attributes, region as IDataConsumer);
            Configure(attributes, region as IDataScopeProvider);
        }

        private void Configure(AttributeSet attributes, IComponent component)
        {
            if (attributes.IsComponent != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsComponent.Name))
                    component.Name = attributes.IsComponent.Name;
            }
            Configure(attributes, component as IElement);
            Configure(attributes, component as IDataConsumer);
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

            Configure(attributes, service as IRunable);
            Configure(attributes, service as IDataScopeProvider);
            Configure(attributes, service as IDataConsumer);
        }

        private void Configure(AttributeSet attributes, IRunable runable)
        {
            if (runable == null) return;

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
                        _requestRouter.Register(runable, new FilterByPath(route.Path), route.Priority, attributes.Type);
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
                                route.Priority,
                                attributes.Type);
                    }
                }
            }
        }

        private void Configure(AttributeSet attributes, IElement element)
        {
            if (element == null) return;

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
        }

        private void Configure(AttributeSet attributes, IDataConsumer dataConsumer)
        {
            if (dataConsumer == null) return;
             
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

        private void Configure(AttributeSet attributes, IDataScopeProvider dataScopeProvider)
        {
            if (dataScopeProvider == null) return;

            if (attributes.DataScopes != null)
            {
                foreach (var dataScope in attributes.DataScopes)
                    dataScopeProvider.AddScope(dataScope.DataType, dataScope.Scope);
            }

            if (attributes.Repeat != null)
            {
                dataScopeProvider.ElementIsProvider(attributes.Repeat.ItemType, attributes.Repeat.ScopeName);
            }
        }

        #endregion
    }
}
