using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
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
        private readonly HashSet<string> _types;
        private readonly IPackage _packageContext;

        public FluentBuilder(
            INameManager nameManager,
            IRequestRouter requestRouter)
        {
            _nameManager = nameManager;
            _requestRouter = requestRouter;
            _assemblies = new HashSet<string>();
            _types = new HashSet<string>();
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

        void IFluentBuilder.Register(IPackage package, string namespaceName)
        {
            if (!_types.Add(package.GetType().FullName))
                return;

            var attributes = package.GetType().GetCustomAttributes(true);

            foreach (var attribute in attributes)
            {
                var isPackage = attribute as IsPackageAttribute;

                if (isPackage != null)
                {
                    if (string.IsNullOrEmpty(package.Name))
                        package.Name = isPackage.Name;

                    if (string.IsNullOrEmpty(package.NamespaceName))
                        package.NamespaceName = isPackage.NamespaceName;
                }
            }

            if (!string.IsNullOrEmpty(namespaceName))
                package.NamespaceName = namespaceName;

            _nameManager.Register(package);

            package.Build(new FluentBuilder(this, package));
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

        void IFluentBuilder.Register(Type type, Func<Type, object> factory)
        {
            if (!_types.Add(type.FullName))
                return;

            var attributes = new AttributeSet(type);

            if (attributes.IsPackage != null) BuildPackage(attributes, factory);
            if (attributes.IsModule != null) BuildModule(attributes, factory);
            if (attributes.IsPage != null) BuildPage(attributes, factory);
            if (attributes.IsLayout != null) BuildLayout(attributes, factory);
            if (attributes.IsRegion != null) BuildRegion(attributes, factory);
            if (attributes.IsComponent != null) BuildComponent(attributes, factory);
            if (attributes.IsService != null) BuildService(attributes, factory);
            if (attributes.IsDataProvider != null) BuildDataProvider(attributes, factory);
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

        private void BuildPackage(AttributeSet attributes, Func<Type, object> factory)
        {
            if (factory != null && typeof(IPackage).IsAssignableFrom(attributes.Type))
            {
                var package = factory(attributes.Type) as IPackage;
                if (package != null)
                {
                    ((IFluentBuilder)this).Register(package);
                    return;
                }
            }

            IPackageDefinition packageDefinition = new PackageDefinition(attributes.Type, this, _nameManager);
            
            packageDefinition.Name(attributes.IsPackage.Name);
            packageDefinition.NamespaceName(attributes.IsPackage.NamespaceName);

            if (attributes.DeployedAs != null)
                packageDefinition.Module(attributes.DeployedAs.ModuleName);

            packageDefinition.Build();
        }

        private void BuildModule(AttributeSet attributes, Func<Type, object> factory)
        {
            if (factory != null && typeof(IModule).IsAssignableFrom(attributes.Type))
            {
                var module = factory(attributes.Type) as IModule;
                if (module != null)
                {
                    if (attributes.IsModule != null)
                    {
                        if (!string.IsNullOrEmpty(attributes.IsModule.Name))
                            module.Name = attributes.IsModule.Name;
                        module.AssetDeployment = attributes.IsModule.AssetDeployment;
                    }
                    _nameManager.Register(module);
                    return;
                }
            }

            var moduleDefinition = Module()
                .Name(attributes.IsModule.Name)
                .AssetDeployment(attributes.IsModule.AssetDeployment);

            moduleDefinition.Build();
        }

        private void BuildPage(AttributeSet attributes, Func<Type, object> factory)
        {
            if (factory != null && typeof(IPage).IsAssignableFrom(attributes.Type))
            {
                var page = factory(attributes.Type) as IPage;
                if (page != null)
                {
                    Configure(attributes, page);
                    return;
                }
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

            pageDefinition.Build();
        }

        private void BuildLayout(AttributeSet attributes, Func<Type, object> factory)
        {
            if (factory != null && typeof(ILayout).IsAssignableFrom(attributes.Type))
            {
                var layout = factory(attributes.Type) as ILayout;
                if (layout != null)
                {
                    Configure(attributes, layout);
                    _nameManager.Register(layout);
                    return;
                }
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
    
            layoutDefinition.Build();
        }

        private void BuildRegion(AttributeSet attributes, Func<Type, object> factory)
        {
            if (factory != null && typeof(IRegion).IsAssignableFrom(attributes.Type))
            {
                var region = factory(attributes.Type) as IRegion;
                if (region != null)
                {
                    Configure(attributes, region);
                    _nameManager.Register(region);
                    return;
                }
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

            regionDefinition.Build();
        }

        private void BuildComponent(AttributeSet attributes, Func<Type, object> factory)
        {
            if (factory != null && typeof(IComponent).IsAssignableFrom(attributes.Type))
            {
                var component = factory(attributes.Type) as IComponent;
                if (component != null)
                {
                    Configure(attributes, component);
                    _nameManager.Register(component);
                    return;
                }
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

            componentDefinition.Build();
        }

        private void BuildService(AttributeSet attributes, Func<Type, object> factory)
        {
            if (factory != null && typeof(IService).IsAssignableFrom(attributes.Type))
            {
                var service = factory(attributes.Type) as IService;
                if (service != null)
                {
                    Configure(attributes, service);
                    _nameManager.Register(service);
                    return;
                }
            }

            var serviceDefinition = Service(attributes.Type, _packageContext)
                .Name(attributes.IsService.Name);

            serviceDefinition.Build();
        }

        private void BuildDataProvider(AttributeSet attributes, Func<Type, object> factory)
        {
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
                        "have more than one [UsesComponent] attribute on " + attributes.Type.FullName);
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
                        "have more than one [UsesLayout] attribute on " + attributes.Type.FullName);
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

            ConfigureElement(attributes, service);
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
        }
    }
}
