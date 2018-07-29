using System;
using System.Linq;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
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
    internal class ElementConfiguror: IElementConfiguror
    {
        private readonly IDataSupplierFactory _dataSupplierFactory;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly INameManager _nameManager;
        private readonly IRequestRouter _requestRouter;

        public ElementConfiguror(
            IDataSupplierFactory dataSupplierFactory, 
            IDataDependencyFactory dataDependencyFactory,
            INameManager nameManager,
            IRequestRouter requestRouter)
        {
            _dataSupplierFactory = dataSupplierFactory;
            _dataDependencyFactory = dataDependencyFactory;
            _nameManager = nameManager;
            _requestRouter = requestRouter;
        }

        #region Using fluent syntax to configure elements that inherit from the framework concrete types

        public void Configure(IComponentDefinition component, AttributeSet attributes)
        {
            if (ReferenceEquals(component, null)) return;

            if (!ReferenceEquals(attributes.IsComponent, null))
            {
                component.Name(attributes.IsComponent.Name);
            }

            if (!ReferenceEquals(attributes.DeployCsss, null))
            {
                foreach (var css in attributes.DeployCsss)
                {
                    component.DeployCss(css.CssSelector, css.CssStyle);
                }
            }

            if (!ReferenceEquals(attributes.DeployFunctions, null))
            {
                foreach (var function in attributes.DeployFunctions)
                {
                    component.DeployFunction(
                        function.ReturnType,
                        function.FunctionName,
                        function.Parameters,
                        function.Body,
                        function.IsPublic);
                }
            }

            if (!ReferenceEquals(attributes.NeedsDatas, null))
            {
                foreach (var dataNeed in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(dataNeed.DataProviderName))
                        component.DataProvider(dataNeed.DataProviderName);
                    if (dataNeed.DataType != null)
                        component.BindTo(dataNeed.DataType, dataNeed.Scope);
                }
            }

            if (!ReferenceEquals(attributes.PartOf, null))
            {
                component.PartOf(attributes.PartOf.PackageName);
            }

            if (!ReferenceEquals(attributes.DeployedAs, null))
            {
                component
                    .DeployIn(attributes.DeployedAs.ModuleName)
                    .AssetDeployment(attributes.DeployedAs.Deployment);
            }

            if (!ReferenceEquals(attributes.NeedsComponents, null))
            {
                foreach(var need in attributes.NeedsComponents)
                {
                    component.NeedsComponent(need.ComponentName);
                }
            }

            if (!ReferenceEquals(attributes.RenderHtmls, null))
            {
                foreach (var html in attributes.RenderHtmls.OrderBy(h => h.Order))
                {
                    component.Render(html.TextName, html.Html);
                }
            }
        }

        public void Configure(IDataProviderDefinition dataProvider, AttributeSet attributes)
        {
            if (ReferenceEquals(dataProvider, null)) return;

            if (!ReferenceEquals(attributes.IsDataProvider, null))
            {
                dataProvider.Name(attributes.IsDataProvider.Name);
                if (attributes.IsDataProvider.Type != null)
                    dataProvider.Provides(attributes.IsDataProvider.Type, attributes.IsDataProvider.Scope);
            }

            if (!ReferenceEquals(attributes.NeedsDatas, null))
            {
                foreach(var dataNeed in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(dataNeed.DataProviderName))
                        dataProvider.DependsOn(dataNeed.DataProviderName);
                    if (dataNeed.DataType != null)
                        dataProvider.BindTo(dataNeed.DataType, dataNeed.Scope);
                }
            }

            if (!ReferenceEquals(attributes.PartOf, null))
            {
                dataProvider.PartOf(attributes.PartOf.PackageName);
            }

            if (!ReferenceEquals(attributes.SuppliesDatas, null))
            {
                foreach(var data in attributes.SuppliesDatas)
                {
                    dataProvider.Provides(data.DataType, data.Scope);
                }
            }
        }

        public void Configure(ILayoutDefinition layout, AttributeSet attributes)
        {
            if (ReferenceEquals(layout, null)) return;

            if (!ReferenceEquals(attributes.IsLayout, null))
            {
                layout
                    .Name(attributes.IsLayout.Name)
                    .RegionNesting(attributes.IsLayout.RegionNesting);
            }

            if (!ReferenceEquals(attributes.NeedsDatas, null))
            {
                foreach (var dataNeed in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(dataNeed.DataProviderName))
                        layout.DataProvider(dataNeed.DataProviderName);
                    if (dataNeed.DataType != null)
                        layout.BindTo(dataNeed.DataType, dataNeed.Scope);
                }
            }

            if (!ReferenceEquals(attributes.PartOf, null))
            {
                layout.PartOf(attributes.PartOf.PackageName);
            }

            if (!ReferenceEquals(attributes.DeployedAs, null))
            {
                layout
                    .DeployIn(attributes.DeployedAs.ModuleName)
                    .AssetDeployment(attributes.DeployedAs.Deployment);
            }

            if (!ReferenceEquals(attributes.NeedsComponents, null))
            {
                foreach (var need in attributes.NeedsComponents)
                {
                    layout.NeedsComponent(need.ComponentName);
                }
            }

            if (!ReferenceEquals(attributes.Container, null))
            {
                layout
                    .Tag(attributes.Container.Tag)
                    .ClassNames(attributes.Container.ClassNames);
            }

            if (!ReferenceEquals(attributes.Style, null))
            {
                layout
                    .Style(attributes.Style.CssStyle);
            }

            if (!ReferenceEquals(attributes.ChildContainer, null))
            {
                layout
                    .NestingTag(attributes.ChildContainer.Tag)
                    .NestedClassNames(attributes.ChildContainer.ClassNames);
            }

            if (!ReferenceEquals(attributes.ChildStyle, null))
            {
                layout.NestedStyle(attributes.ChildStyle.CssStyle);
            }

            if (!ReferenceEquals(attributes.DeployCsss, null))
            {
                foreach(var css in attributes.DeployCsss)
                {
                    layout.DeployCss(css.CssSelector, css.CssStyle);
                }
            }

            if (!ReferenceEquals(attributes.DeployFunctions, null))
            { 
                foreach(var function in attributes.DeployFunctions)
                {
                    layout.DeployFunction(
                        function.ReturnType, 
                        function.FunctionName, 
                        function.Parameters, 
                        function.Body, 
                        function.IsPublic);
                }
            }

            if (!ReferenceEquals(attributes.UsesRegions, null))
            {
                foreach (var usesRegion in attributes.UsesRegions)
                {
                    layout.Region(usesRegion.RegionName, usesRegion.RegionElement);
                }
            }

            if (!ReferenceEquals(attributes.RegionComponents, null))
            {
                foreach(var regionComponent in attributes.RegionComponents)
                {
                    layout.Component(regionComponent.Region, regionComponent.Component);
                }
            }

            if (!ReferenceEquals(attributes.RegionLayouts, null))
            {
                foreach (var regionLayout in attributes.RegionLayouts)
                {
                    layout.Layout(regionLayout.Region, regionLayout.Layout);
                }
            }
        }

        public void Configure(IModuleDefinition module, AttributeSet attributes)
        {
            if (ReferenceEquals(module, null)) return;

            if (!ReferenceEquals(attributes.IsModule, null))
            {
                module
                    .Name(attributes.IsModule.Name)
                    .AssetDeployment(attributes.IsModule.AssetDeployment);
            }
        }

        public void Configure(IPackageDefinition package, AttributeSet attributes)
        {
            if (ReferenceEquals(package, null)) return;

            if (!ReferenceEquals(attributes.IsPackage, null))
            {
                package
                    .Name(attributes.IsPackage.Name)
                    .NamespaceName(attributes.IsPackage.NamespaceName);
            }

            if (!ReferenceEquals(attributes.DeployedAs, null))
            {
                package.Module(attributes.DeployedAs.ModuleName);
            }
        }

        public void Configure(IPageDefinition page, AttributeSet attributes)
        {
            if (ReferenceEquals(page, null)) return;

            if (!ReferenceEquals(attributes.IsPage, null))
            {
                page.Name(attributes.IsPage.Name);
            }

            if (!ReferenceEquals(attributes.NeedsDatas, null))
            {
                foreach (var dataNeed in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(dataNeed.DataProviderName))
                        page.DataProvider(dataNeed.DataProviderName);
                    if (dataNeed.DataType != null)
                        page.BindTo(dataNeed.DataType, dataNeed.Scope);
                }
            }

            if (!ReferenceEquals(attributes.PartOf, null))
            {
                page.PartOf(attributes.PartOf.PackageName);
            }

            if (!ReferenceEquals(attributes.DeployedAs, null))
            {
                page
                    .DeployIn(attributes.DeployedAs.ModuleName)
                    .AssetDeployment(attributes.DeployedAs.Deployment);
            }

            if (!ReferenceEquals(attributes.NeedsComponents, null))
            {
                foreach (var need in attributes.NeedsComponents)
                {
                    page.NeedsComponent(need.ComponentName);
                }
            }

            if (!ReferenceEquals(attributes.RegionComponents, null))
            {
                foreach (var regionComponent in attributes.RegionComponents)
                {
                    page.RegionComponent(regionComponent.Region, regionComponent.Component);
                }
            }

            if (!ReferenceEquals(attributes.RegionLayouts, null))
            {
                foreach (var regionLayout in attributes.RegionLayouts)
                {
                    page.RegionLayout(regionLayout.Region, regionLayout.Layout);
                }
            }

            if (!ReferenceEquals(attributes.PageTitle, null))
            {
                page.Title(attributes.PageTitle.Title);
            }

            if (!ReferenceEquals(attributes.Style, null))
            {
                page.BodyStyle(attributes.Style.CssStyle);
            }

            if (!ReferenceEquals(attributes.UsesLayouts, null) && attributes.UsesLayouts.Count > 0)
            {
                if (attributes.UsesLayouts.Count > 1)
                    throw new FluentBuilderException("A page can not have more than one layout");

                page.Layout(attributes.UsesLayouts[0].LayoutName);
            }

            if (!ReferenceEquals(attributes.Routes, null))
            {
                foreach (var route in attributes.Routes)
                {
                    page.Route(route.Path, route.Priority, route.Methods);
                }
            }

            if (!ReferenceEquals(attributes.DataScopes, null))
            {
                foreach (var dataScope in attributes.DataScopes)
                {
                    page.DataScope(dataScope.DataType, dataScope.Scope);
                }
            }
        }

        public void Configure(IRegionDefinition region, AttributeSet attributes)
        {
            if (ReferenceEquals(region, null)) return;

            if (!ReferenceEquals(attributes.IsRegion, null))
            {
                region.Name(attributes.IsRegion.Name);
            }

            if (!ReferenceEquals(attributes.NeedsDatas, null))
            {
                foreach (var dataNeed in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(dataNeed.DataProviderName))
                        region.DataProvider(dataNeed.DataProviderName);
                    if (dataNeed.DataType != null)
                        region.BindTo(dataNeed.DataType, dataNeed.Scope);
                }
            }

            if (!ReferenceEquals(attributes.PartOf, null))
            {
                region.PartOf(attributes.PartOf.PackageName);
            }

            if (!ReferenceEquals(attributes.DeployedAs, null))
            {
                region
                    .DeployIn(attributes.DeployedAs.ModuleName)
                    .AssetDeployment(attributes.DeployedAs.Deployment);
            }

            if (!ReferenceEquals(attributes.NeedsComponents, null))
            {
                foreach (var need in attributes.NeedsComponents)
                {
                    region.NeedsComponent(need.ComponentName);
                }
            }

            if (!ReferenceEquals(attributes.Style, null))
            {
                region.Style(attributes.Style.CssStyle);
            }

            if (!ReferenceEquals(attributes.UsesLayouts, null) && attributes.UsesLayouts.Count > 0)
            {
                if (attributes.UsesLayouts.Count > 1)
                    throw new FluentBuilderException("A region can not have more than one layout");

                region.Layout(attributes.UsesLayouts[0].LayoutName);
            }

            if (!ReferenceEquals(attributes.UsesComponents, null) && attributes.UsesComponents.Count > 0)
            {
                if (!ReferenceEquals(attributes.UsesLayouts, null) && attributes.UsesLayouts.Count > 0)
                    throw new FluentBuilderException("A region can contain a layout or a component, but not both");

                if (attributes.UsesComponents.Count > 1)
                    throw new FluentBuilderException("A region can not have more than one component");

                region.Component(attributes.UsesComponents[0].ComponentName);
            }

            if (!ReferenceEquals(attributes.DataScopes, null))
            {
                foreach (var dataScope in attributes.DataScopes)
                {
                    region.DataScope(dataScope.DataType, dataScope.Scope);
                }
            }

            if (!ReferenceEquals(attributes.Container, null))
            {
                region
                    .Tag(attributes.Container.Tag)
                    .ClassNames(attributes.Container.ClassNames);
            }

            if (!ReferenceEquals(attributes.Style, null))
            {
                region.Style(attributes.Style.CssStyle);
            }

            if (!ReferenceEquals(attributes.Repeat, null))
            {
                region.ForEach(
                    attributes.Repeat.RepeatType,
                    attributes.Repeat.RepeatScope,
                    attributes.Repeat.Tag,
                    attributes.Repeat.Style,
                    attributes.Repeat.ListScope,
                    attributes.Repeat.ClassNames);
            }
        }

        public void Configure(IServiceDefinition service, AttributeSet attributes)
        {
            if (ReferenceEquals(service, null)) return;

            if (!ReferenceEquals(attributes.IsService, null))
            {
                service.Name(attributes.IsService.Name);
            }

            if (!ReferenceEquals(attributes.NeedsDatas, null))
            {
                foreach (var dataNeed in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(dataNeed.DataProviderName))
                        service.DataProvider(dataNeed.DataProviderName);
                    if (dataNeed.DataType != null)
                        service.BindTo(dataNeed.DataType, dataNeed.Scope);
                }
            }

            if (!ReferenceEquals(attributes.PartOf, null))
            {
                service.PartOf(attributes.PartOf.PackageName);
            }

            if (!ReferenceEquals(attributes.DeployedAs, null))
            {
                service
                    .DeployIn(attributes.DeployedAs.ModuleName)
                    .AssetDeployment(attributes.DeployedAs.Deployment);
            }

            if (!ReferenceEquals(attributes.Routes, null))
            {
                foreach (var route in attributes.Routes)
                {
                    service.Route(route.Path, route.Priority, route.Methods);
                }
            }

            if (!ReferenceEquals(attributes.DataScopes, null))
            {
                foreach (var dataScope in attributes.DataScopes)
                {
                    service.DataScope(dataScope.DataType, dataScope.Scope);
                }
            }
        }

        #endregion

        #region Configuring custom elements that directly implement the interfaces

        public void Configure(object element, AttributeSet attributes)
        {
            Configure(attributes, element as IElement);
            Configure(attributes, element as IComponent);
            Configure(attributes, element as IRegion);
            Configure(attributes, element as ILayout);
            Configure(attributes, element as IPage);
            Configure(attributes, element as IService);
            Configure(attributes, element as IModule);
            Configure(attributes, element as IPackage);
            Configure(attributes, element as IDataProvider);

            Configure(attributes, element as IDataScopeProvider);
            Configure(attributes, element as IDataConsumer);
            Configure(attributes, element as IDataSupplier);
            Configure(attributes, element as IRunable);
            Configure(attributes, element as IPackagable);
            Configure(attributes, element as IDeployable);
            Configure(attributes, element as IDataRepeater);
        }

        public void Configure(IComponent component, AttributeSet attributes)
        {
            Configure((object)component, attributes);
        }

        public void Configure(IDataProvider dataProvider, AttributeSet attributes)
        {
            Configure((object)dataProvider, attributes);
        }

        public void Configure(ILayout layout, AttributeSet attributes)
        {
            Configure((object)layout, attributes);
        }

        public void Configure(IModule module, AttributeSet attributes)
        {
            Configure((object)module, attributes);
        }

        public void Configure(IPackage package, AttributeSet attributes)
        {
            Configure((object)package, attributes);
        }

        public void Configure(IPage page, AttributeSet attributes)
        {
            Configure((object)page, attributes);
        }

        public void Configure(IRegion region, AttributeSet attributes)
        {
            Configure((object)region, attributes);
        }

        public void Configure(IService service, AttributeSet attributes)
        {
            Configure((object)service, attributes);
        }
        
        #endregion

        #region Specific element types

        private void Configure(AttributeSet attributes, IElement element)
        {
            // All elements are IDeployable and IPackagable so they
            // are taken care of by the generic configuration methods
        }

        private void Configure(AttributeSet attributes, IComponent component)
        {
            if (component == null) return;

            if (attributes.IsComponent != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsComponent.Name))
                    component.Name = attributes.IsComponent.Name;
            }
        }

        private void Configure(AttributeSet attributes, IRegion region)
        {
            if (region == null) return;

            if (attributes.IsRegion != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsRegion.Name))
                    region.Name = attributes.IsRegion.Name;
            }

            var hasComponent = false;

            if (attributes.UsesComponents != null)
            {
                if (attributes.UsesComponents.Count > 1)
                    throw new RegionBuilderException("Regions can only host one component but you " +
                        "have more than one [UsesComponent] attribute on " + attributes.Type.DisplayName());

                if (attributes.UsesComponents.Count == 1)
                {
                    hasComponent = true;

                    _nameManager.AddResolutionHandler(
                        NameResolutionPhase.ResolveElementReferences,
                        (nm, r, n) => r.Populate(nm.ResolveComponent(n, r.Package)),
                        region,
                        attributes.UsesComponents[0].ComponentName);
                }
            }

            if (attributes.UsesLayouts != null)
            {
                if (hasComponent)
                    throw new RegionBuilderException("Regions can only host one element but you " +
                        "have both [UsesComponent] and [UsesLayout] attributes on " + attributes.Type.DisplayName());

                if (attributes.UsesLayouts.Count > 1)
                    throw new RegionBuilderException("Regions can only host one layout but you " +
                        "have more than one [UsesLayout] attribute on " + attributes.Type.DisplayName());

                if (attributes.UsesLayouts.Count == 1)
                {
                    _nameManager.AddResolutionHandler(
                        NameResolutionPhase.ResolveElementReferences,
                        (nm, r, n) => r.Populate(nm.ResolveLayout(n, r.Package)),
                        region,
                        attributes.UsesLayouts[0].LayoutName);
                }
            }
        }

        private void Configure(AttributeSet attributes, ILayout layout)
        {
            if (layout == null) return;

            if (attributes.IsLayout != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsLayout.Name))
                    layout.Name = attributes.IsLayout.Name;
            }

            if (attributes.RegionComponents != null)
            {
                foreach (var regionComponent in attributes.RegionComponents)
                {
                    _nameManager.AddResolutionHandler(
                        NameResolutionPhase.ResolveElementReferences,
                        (nm, l, rc) => l.Populate(rc.Region, nm.ResolveComponent(rc.Component, l.Package)),
                        layout,
                        regionComponent);
                }
            }

            if (attributes.RegionLayouts != null)
            {
                foreach (var regionLayout in attributes.RegionLayouts)
                {
                    _nameManager.AddResolutionHandler(
                        NameResolutionPhase.ResolveElementReferences,
                        (nm, l, rl) => l.Populate(rl.Region, nm.ResolveComponent(rl.Layout, l.Package)),
                        layout,
                        regionLayout);
                }
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
        }

        private void Configure(AttributeSet attributes, IService service)
        {
            if (service == null) return;

            if (attributes.IsService != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsService.Name))
                    service.Name = attributes.IsService.Name;
            }
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

        private void Configure(AttributeSet attributes, IPackage package)
        {
            if (package == null) return;

            if (attributes.IsPackage != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsPackage.Name))
                    package.Name = attributes.IsPackage.Name;

                if (string.IsNullOrEmpty(package.NamespaceName))
                    package.NamespaceName = attributes.IsPackage.NamespaceName;
            }
        }

        private void Configure(AttributeSet attributes, IDataProvider dataProvider)
        {
            if (dataProvider == null) return;

            if (attributes.IsDataProvider != null)
            {
                if (!string.IsNullOrEmpty(attributes.IsDataProvider.Name))
                    dataProvider.Name = attributes.IsDataProvider.Name;

                if (attributes.IsDataProvider.Type != null)
                {
                    dataProvider.Add(
                        attributes.IsDataProvider.Type,
                        attributes.IsDataProvider.Scope);
                }
            }
        }

        #endregion

        #region Generic capabilities

        private void Configure(AttributeSet attributes, IPackagable packagable)
        {
            if (packagable == null) return;

            if (attributes.PartOf != null && !string.IsNullOrEmpty(attributes.PartOf.PackageName))
            {
                _nameManager.AddResolutionHandler(
                    NameResolutionPhase.ResolvePackageNames,
                    (nm, e, n) => e.Package = nm.ResolvePackage(n),
                    packagable,
                    attributes.PartOf.PackageName);
            }
        }

        private void Configure(AttributeSet attributes, IDeployable deployable)
        {
            if (deployable == null) return;

            if (attributes.DeployedAs != null)
            {
                deployable.AssetDeployment = attributes.DeployedAs.Deployment;

                var moduleName = attributes.DeployedAs.ModuleName;
                if (!string.IsNullOrEmpty(moduleName))
                {
                    _nameManager.AddResolutionHandler(
                        NameResolutionPhase.ResolveElementReferences,
                        (nm, d, n) => d.Module = nm.ResolveModule(n),
                        deployable,
                        moduleName);
                }
            }
        }

        private void Configure(AttributeSet attributes, IDataRepeater dataRepeater)
        {
            if (dataRepeater == null) return;

            if (attributes.Repeat != null)
            {
                dataRepeater.RepeatScope = attributes.Repeat.RepeatScope;
                dataRepeater.RepeatType = attributes.Repeat.RepeatType;
                dataRepeater.ListScope = attributes.Repeat.ListScope;
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
                        dataConsumer.HasDependency(need.DataType, need.Scope);

                    if (!string.IsNullOrEmpty(need.DataProviderName))
                    {
                        _nameManager.AddResolutionHandler(
                            NameResolutionPhase.ResolveElementReferences,
                            (nm, dc, n) => dc.HasDependency(nm.ResolveDataProvider(n)),
                            dataConsumer,
                            need.DataProviderName);
                    }
                }
            }
        }

        private void Configure(AttributeSet attributes, IDataSupplier dataSupplier)
        {
            if (dataSupplier == null) return;

            if (attributes.SuppliesDatas != null)
            {
                foreach (var data in attributes.SuppliesDatas)
                {
                    var dependency = _dataDependencyFactory.Create(
                        data.DataType,
                        data.Scope);

                    // TODO: Is there a way to specify the action via attributes?
                    dataSupplier.Add(dependency, (rc, dc, dep) => { });
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
                // TODO: This is no longer correct



                // When data scope providers repeat data they effectively are a supplier
                // of the data they repeat, but the supplier itself does not add the
                // data to the data context, it is added by the repeating action during
                // the rendering operation. We need to add a supplier here otherwide the
                // data scope provider will try to resolve dependencies on the repeated
                // data by looking in the data catalog.

                var dependency = _dataDependencyFactory.Create(attributes.Repeat.RepeatType, attributes.Repeat.RepeatScope);
                var supplier = _dataSupplierFactory.Create();
                supplier.Add(dependency, (rc, dc, d) => { });

                dataScopeProvider.AddScope(attributes.Repeat.RepeatType, attributes.Repeat.RepeatScope);
                dataScopeProvider.AddSupplier(supplier, dependency, null);
            }
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
                        {
                            _requestRouter.Register(runable, new FilterByMethod(route.Methods), route.Priority);
                        }
                        else
                        {
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
        }

        #endregion
    }
}
