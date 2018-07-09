using System;
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

        #region Specific element types

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
                    var componentName = attributes.UsesComponents[0].ComponentName;

                    _nameManager.AddResolutionHandler((nm, r) => 
                        r.Populate(nm.ResolveComponent(componentName, r.Package)),
                        region);
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
                    var layoutName = attributes.UsesLayouts[0].LayoutName;
                    _nameManager.AddResolutionHandler((nm, r) => 
                        r.Populate(nm.ResolveLayout(layoutName, r.Package)),
                        region);
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

                layout.RegionNesting(attributes.IsLayout.RegionNesting);
            }

            if (attributes.RegionComponents != null)
            {
                foreach (var regionComponent in attributes.RegionComponents)
                {
                    var rc = regionComponent;
                    _nameManager.AddResolutionHandler((nm, l) =>
                        l.Populate(rc.Region, nm.ResolveComponent(rc.Component, l.Package)), 
                        layout);
                }
            }

            if (attributes.RegionLayouts != null)
            {
                foreach (var regionLayout in attributes.RegionLayouts)
                {
                    var rl = regionLayout;
                    _nameManager.AddResolutionHandler((nm, l) =>
                        l.Populate(rl.Region, nm.ResolveLayout(rl.Layout, l.Package)), 
                        layout);
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
                    var dependency = _dataDependencyFactory.Create(
                        attributes.IsDataProvider.Type, 
                        attributes.IsDataProvider.Scope);

                    // TODO: Is there a way to specify the action via attributes?
                    dataProvider.Add(dependency, (rc, dc, dep) => { });
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
                packagable.Package = _nameManager.ResolvePackage(attributes.PartOf.PackageName);
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
                    _nameManager.AddResolutionHandler((nm, d) =>
                        d.Module = nm.ResolveModule(moduleName),
                        deployable);
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
                        _nameManager.AddResolutionHandler(
                            (nm, dc) => dc.HasDependency(nm.ResolveDataProvider(need.DataProviderName)),
                            dataConsumer);
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
                dataScopeProvider.AddSupplier(supplier, dependency);
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

        #endregion
    }
}
