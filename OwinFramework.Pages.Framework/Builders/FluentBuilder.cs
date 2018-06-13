using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

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
        private readonly HashSet<string> _assemblies;
        private readonly HashSet<string> _types;
        private readonly IPackage _packageContext;

        public FluentBuilder(
            INameManager nameManager)
        {
            _nameManager = nameManager;
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

        void IFluentBuilder.Register(Assembly assembly)
        {
            if (!_assemblies.Add(assembly.FullName))
                return;

            var types = assembly.GetTypes();

            var packageTypes = types.Where(t => t.GetCustomAttributes(true).Any(a => a is IsPackageAttribute)).ToList();
            var otherTypes = types.Where(t => !t.GetCustomAttributes(true).Any(a => a is IsPackageAttribute)).ToList();

            Exception exception = null;

            // Must register packages first because they define the 
            // namespace for the other elements
            foreach (var type in packageTypes)
            {
                try
                {
                    ((IFluentBuilder)this).Register(type);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            // Register everything else
            foreach (var type in otherTypes)
            {
                try
                {
                    ((IFluentBuilder)this).Register(type);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            if (exception != null)
                throw exception;
        }

        void IFluentBuilder.Register(Type type)
        {
            if (!_types.Add(type.FullName))
                return;

            var attributes = type.GetCustomAttributes(true);

            IsPackageAttribute isPackage = null;
            IsModuleAttribute isModule = null;
            IsPageAttribute isPage = null;
            IsLayoutAttribute isLayout = null;
            IsRegionAttribute isRegion = null;
            IsComponentAttribute isComponent = null;
            IsServiceAttribute isService = null;

            foreach (var attribute in attributes)
            {
                if (attribute is IsPackageAttribute)
                    isPackage = attribute as IsPackageAttribute;

                if (attribute is IsModuleAttribute)
                    isModule = attribute as IsModuleAttribute;

                if (attribute is IsPageAttribute)
                    isPage = attribute as IsPageAttribute;

                if (attribute is IsLayoutAttribute)
                    isLayout = attribute as IsLayoutAttribute;

                if (attribute is IsRegionAttribute)
                    isRegion = attribute as IsRegionAttribute;

                if (attribute is IsComponentAttribute)
                    isComponent = attribute as IsComponentAttribute;

                if (attribute is IsServiceAttribute)
                    isService = attribute as IsServiceAttribute;
            }

            if (isPackage != null) BuildPackage(type, isPackage, attributes);
            if (isModule != null) BuildModule(type, isModule, attributes);
            if (isPage != null) BuildPage(type, isPage, attributes);
            if (isLayout != null) BuildLayout(type, isLayout, attributes);
            if (isRegion != null) BuildRegion(type, isRegion, attributes);
            if (isComponent != null) BuildComponent(type, isComponent, attributes);
            if (isService != null) BuildService(type, isService, attributes);
        }

        public IComponentDefinition Component()
        {
            if (ComponentBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build components");
            return ComponentBuilder.Component();
        }

        public IRegionDefinition Region()
        {
            if (RegionBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build regions");
            return RegionBuilder.Region();
        }

        public ILayoutDefinition Layout()
        {
            if (LayoutBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build layouts");
            return LayoutBuilder.Layout();
        }

        public IPageDefinition Page(Type declaringType)
        {
            if (PageBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build pages");
            return PageBuilder.Page(declaringType);
        }

        public IServiceDefinition Service(Type declaringType)
        {
            if (ServiceBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build services");
            return ServiceBuilder.Service();
        }

        public IModuleDefinition Module()
        {
            if (ModuleBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build modules");
            return ModuleBuilder.Module();
        }

        private void BuildPackage(Type type, IsPackageAttribute isPackage, object[] attributes)
        {
            IPackageDefinition package = new PackageDefinition(type, this, _nameManager);
            
            package.Name(isPackage.Name);
            package.NamespaceName(isPackage.NamespaceName);

            foreach (var attribute in attributes)
            {
                var deployedAs = attribute as DeployedAsAttribute;

                if (deployedAs != null)
                    package.Module(deployedAs.ModuleName);
            }

            package.Build();
        }

        private void BuildModule(Type type, IsModuleAttribute isModule, object[] attributes)
        {
            var module = Module()
                .Name(isModule.Name)
                .AssetDeployment(isModule.AssetDeployment);

            module.Build();
        }

        private void BuildPage(Type type, IsPageAttribute isPage, object[] attributes)
        {
            var page = Page(type).Name(isPage.Name);

            if (_packageContext != null)
                page.PartOf(_packageContext);

            foreach (var attribute in attributes)
            {
                var deployedAs = attribute as DeployedAsAttribute;
                var usesLayout = attribute as UsesLayoutAttribute;
                var partOf = attribute as PartOfAttribute;
                var regionComponent = attribute as RegionComponentAttribute;
                var regionLayout = attribute as RegionLayoutAttribute;
                var route = attribute as RouteAttribute;
                var title = attribute as PageTitleAttribute;
                var style = attribute as StyleAttribute;
                var needsData = attribute as NeedsDataAttribute;

                if (deployedAs != null)
                {
                    page.AssetDeployment(deployedAs.Deployment)
                        .Module(deployedAs.ModuleName);
                }

                if (usesLayout != null)
                    page.Layout(usesLayout.LayoutName);

                if (partOf != null)
                    page.PartOf(partOf.PackageName);

                if (needsData != null)
                {
                    if (!string.IsNullOrEmpty(needsData.DataContextName))
                        page.DataContext(needsData.DataContextName);

                    if (needsData.DataType != null)
                        page.BindTo(needsData.DataType);
                }

                if (regionComponent != null)
                    page.Component(regionComponent.Region, regionComponent.Component);

                if (regionLayout != null)
                    page.RegionLayout(regionLayout.Region, regionLayout.Layout);

                if (route != null)
                {
                    page.Path(route.Path);
                    page.Methods(route.Methods);
                }

                if (title != null)
                    page.Title(title.Title);

                if (style != null)
                    page.BodyStyle(style.CssStyle);
            }

            page.Build();
        }

        private void BuildLayout(Type type, IsLayoutAttribute isLayout, object[] attributes)
        {
            var layout = Layout()
                .Name(isLayout.Name)
                .RegionNesting(isLayout.RegionNesting);

            if (_packageContext != null)
                layout.PartOf(_packageContext);

            foreach (var attribute in attributes)
            {
                var deployedAs = attribute as DeployedAsAttribute;
                var partOf = attribute as PartOfAttribute;
                var regionComponent = attribute as RegionComponentAttribute;
                var regionLayout = attribute as RegionLayoutAttribute;
                var style = attribute as StyleAttribute;
                var childStyle = attribute as ChildStyleAttribute;
                var container = attribute as ContainerAttribute;
                var childContainer = attribute as ChildContainerAttribute;
                var usesRegion = attribute as UsesRegionAttribute;
                var needsData = attribute as NeedsDataAttribute;

                if (deployedAs != null)
                    layout.AssetDeployment(deployedAs.Deployment)
                        .DeployIn(deployedAs.ModuleName);

                if (partOf != null)
                    layout.PartOf(partOf.PackageName);

                if (needsData != null)
                {
                    if (!string.IsNullOrEmpty(needsData.DataContextName))
                        layout.DataContext(needsData.DataContextName);

                    if (needsData.DataType != null)
                        layout.BindTo(needsData.DataType);
                }

                if (regionComponent != null)
                    layout.Component(regionComponent.Region, regionComponent.Component);

                if (regionLayout != null)
                    layout.Layout(regionLayout.Region, regionLayout.Layout);

                if (style != null)
                {
                    if (!string.IsNullOrEmpty(style.CssStyle))
                        layout.Style(style.CssStyle);
                }

                if (childStyle != null)
                {
                    if (!string.IsNullOrEmpty(childStyle.CssStyle))
                        layout.NestedStyle(childStyle.CssStyle);
                }

                if (container != null)
                {
                    if (!string.IsNullOrEmpty(container.Tag))
                        layout.Tag(container.Tag);

                    if (container.ClassNames != null && container.ClassNames.Length > 0)
                        layout.ClassNames(container.ClassNames);
                }

                if (childContainer != null)
                {
                    if (!string.IsNullOrEmpty(childContainer.Tag))
                        layout.NestingTag(childContainer.Tag);

                    if (childContainer.ClassNames != null && childContainer.ClassNames.Length > 0)
                        layout.NestedClassNames(childContainer.ClassNames);
                }

                if (usesRegion != null)
                    layout.Region(usesRegion.RegionName, usesRegion.RegionElement);
            }

            layout.Build();
        }

        private void BuildRegion(Type type, IsRegionAttribute isRegion, object[] attributes)
        {
            var region = Region()
                .Name(isRegion.Name);

            if (_packageContext != null)
                region.PartOf(_packageContext);

            foreach (var attribute in attributes)
            {
                var deployedAs = attribute as DeployedAsAttribute;
                var partOf = attribute as PartOfAttribute;
                var style = attribute as StyleAttribute;
                var container = attribute as ContainerAttribute;
                var usesLayout = attribute as UsesLayoutAttribute;
                var usesComponent = attribute as UsesComponentAttribute;
                var needsData = attribute as NeedsDataAttribute;
                var repeat = attribute as RepeatAttribute;

                if (deployedAs != null)
                    region.AssetDeployment(deployedAs.Deployment)
                        .DeployIn(deployedAs.ModuleName);

                if (needsData != null)
                {
                    if (!string.IsNullOrEmpty(needsData.DataContextName))
                        region.DataContext(needsData.DataContextName);

                    if (needsData.DataType != null)
                        region.BindTo(needsData.DataType);
                }
                
                if (partOf != null)
                    region.PartOf(partOf.PackageName);

                if (style != null)
                {
                    if (!string.IsNullOrEmpty(style.CssStyle))
                        region.Style(style.CssStyle);
                }

                if (repeat != null)
                    region.ForEach(repeat.ItemType, repeat.Tag, repeat.Style, repeat.ClassNames);

                if (container != null)
                {
                    if (!string.IsNullOrEmpty(container.Tag))
                        region.Tag(container.Tag);

                    if (container.ClassNames != null && container.ClassNames.Length > 0)
                        region.ClassNames(container.ClassNames);
                }

                if (usesLayout != null)
                    region.Layout(usesLayout.LayoutName);

                if (usesComponent != null)
                    region.Component(usesComponent.ComponentName);
            }

            region.Build();
        }

        private void BuildComponent(Type type, IsComponentAttribute isComponent, object[] attributes)
        {
            var component = Component()
                .Name(isComponent.Name);

            if (_packageContext != null)
                component.PartOf(_packageContext);

            foreach (var attribute in attributes)
            {
                var deployedAs = attribute as DeployedAsAttribute;
                var partOf = attribute as PartOfAttribute;
                var needsData = attribute as NeedsDataAttribute;
                var renderHtml = attribute as RenderHtml;

                if (deployedAs != null)
                    component.AssetDeployment(deployedAs.Deployment)
                        .DeployIn(deployedAs.ModuleName);

                if (partOf != null)
                    component.PartOf(partOf.PackageName);

                if (needsData != null)
                {
                    if (!string.IsNullOrEmpty(needsData.DataContextName))
                        component.DataContext(needsData.DataContextName);

                    if (needsData.DataType != null)
                        component.BindTo(needsData.DataType);
                }

                if (renderHtml != null)
                    component.Render(renderHtml.TextName, renderHtml.Html);
            }

            component.Build();
        }

        private void BuildService(Type type, IsServiceAttribute isService, object[] attributes)
        {
            var service = Service(type).Name(isService.Name);

            service.Build();
        }

        private class PackageDefinition: IPackageDefinition
        {
            private readonly IFluentBuilder _builder;
            private readonly INameManager _nameManager;
            private readonly BuiltPackage _package;
            private readonly Type _declaringType;

            public PackageDefinition(
                Type declaringType,
                IFluentBuilder builder,
                INameManager nameManager)
            {
                _declaringType = declaringType;
                _builder = builder;
                _nameManager = nameManager;
                _package = new BuiltPackage();
            }

            IPackageDefinition IPackageDefinition.Name(string name)
            {
                _package.Name = name;
                return this;
            }

            IPackageDefinition IPackageDefinition.NamespaceName(string namespaceName)
            {
                _package.NamespaceName = namespaceName;
                return this;
            }

            IPackageDefinition IPackageDefinition.Module(string moduleName)
            {
                _nameManager.AddResolutionHandler(nm => _package.Module = nm.ResolveModule("moduleName"));
                return this;
            }

            IPackageDefinition IPackageDefinition.Module(IModule module)
            {
                _package.Module = module;
                return this;
            }

            IPackage IPackageDefinition.Build()
            {
                _builder.Register(_package);
                return _package;
            }
        }

        private class BuiltPackage : IPackage
        {
            public string Name { get; set; }
            public string NamespaceName { get; set; }
            public IModule Module { get; set; }

            public IPackage Build(IFluentBuilder builder)
            {
                // Packages defined declaratively do not build anything within them
                return this;
            }
        }

    }
}
