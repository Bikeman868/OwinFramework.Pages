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

            var attributes = new AttributeSet(type);

            if (attributes.IsPackage != null) BuildPackage(attributes);
            if (attributes.IsModule != null) BuildModule(attributes);
            if (attributes.IsPage != null) BuildPage(attributes);
            if (attributes.IsLayout != null) BuildLayout(attributes);
            if (attributes.IsRegion != null) BuildRegion(attributes);
            if (attributes.IsComponent != null) BuildComponent(attributes);
            if (attributes.IsService != null) BuildService(attributes);
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

        private void BuildPackage(AttributeSet attributes)
        {
            IPackageDefinition package = new PackageDefinition(attributes.Type, this, _nameManager);
            
            package.Name(attributes.IsPackage.Name);
            package.NamespaceName(attributes.IsPackage.NamespaceName);

            if (attributes.DeployedAs != null)
                package.Module(attributes.DeployedAs.ModuleName);

            package.Build();
        }

        private void BuildModule(AttributeSet attributes)
        {
            var module = Module()
                .Name(attributes.IsModule.Name)
                .AssetDeployment(attributes.IsModule.AssetDeployment);

            module.Build();
        }

        private void BuildPage(AttributeSet attributes)
        {
            var page = Page(attributes.Type)
                .Name(attributes.IsPage.Name);

            if (_packageContext != null)
                page.PartOf(_packageContext);

            if (attributes.DeployedAs != null)
                page.AssetDeployment(attributes.DeployedAs.Deployment)
                    .Module(attributes.DeployedAs.ModuleName);

            if (attributes.UsesLayouts != null)
                foreach(var usesLayout in attributes.UsesLayouts)
                    page.Layout(usesLayout.LayoutName);

            if (attributes.PartOf != null)
                page.PartOf(attributes.PartOf.PackageName);

            if (attributes.PageTitle != null)
                page.Title(attributes.PageTitle.Title);

            if (attributes.Style != null)
                page.BodyStyle(attributes.Style.CssStyle);

            if (attributes.NeedsDatas != null)
            {
                foreach (var needsData in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(needsData.DataContextName))
                        page.DataContext(needsData.DataContextName);

                    if (needsData.DataType != null)
                        page.BindTo(needsData.DataType);
                }
            }

            if (attributes.RegionComponents != null)
            {
                foreach (var regionComponent in attributes.RegionComponents)
                {
                    page.Component(regionComponent.Region, regionComponent.Component);
                }
            }

            if (attributes.RegionLayouts != null)
            {
                foreach (var regionLayout in attributes.RegionLayouts)
                {
                    page.RegionLayout(regionLayout.Region, regionLayout.Layout);
                }
            }

            if (attributes.Routes != null)
            {
                foreach(var route in attributes.Routes)
                {
                    page.Route(route.Path, route.Priority, route.Methods);
                }
            }

            page.Build();
        }

        private void BuildLayout(AttributeSet attributes)
        {
            var layout = Layout()
                .Name(attributes.IsLayout.Name)
                .RegionNesting(attributes.IsLayout.RegionNesting);

            if (_packageContext != null)
                layout.PartOf(_packageContext);

            if (attributes.DeployedAs != null)
                layout.AssetDeployment(attributes.DeployedAs.Deployment)
                    .DeployIn(attributes.DeployedAs.ModuleName);

            if (attributes.PartOf != null)
                layout.PartOf(attributes.PartOf.PackageName);

            if (attributes.NeedsDatas != null)
            {
                foreach (var needsData in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(needsData.DataContextName))
                        layout.DataContext(needsData.DataContextName);

                    if (needsData.DataType != null)
                        layout.BindTo(needsData.DataType);
                }
            }

            if (attributes.RegionComponents != null)
                foreach(var regionComponent in attributes.RegionComponents)
                    layout.Component(regionComponent.Region, regionComponent.Component);

            if (attributes.RegionLayouts != null)
                foreach (var regionLayout in attributes.RegionLayouts)
                    layout.Layout(regionLayout.Region, regionLayout.Layout);

            if (attributes.Style != null)
            {
                if (!string.IsNullOrEmpty(attributes.Style.CssStyle))
                    layout.Style(attributes.Style.CssStyle);
            }

            if (attributes.ChildStyle != null)
            {
                if (!string.IsNullOrEmpty(attributes.ChildStyle.CssStyle))
                    layout.NestedStyle(attributes.ChildStyle.CssStyle);
            }

            if (attributes.Container != null)
            {
                if (!string.IsNullOrEmpty(attributes.Container.Tag))
                    layout.Tag(attributes.Container.Tag);

                if (attributes.Container.ClassNames != null && attributes.Container.ClassNames.Length > 0)
                    layout.ClassNames(attributes.Container.ClassNames);
            }

            if (attributes.ChildContainer != null)
            {
                if (!string.IsNullOrEmpty(attributes.ChildContainer.Tag))
                    layout.NestingTag(attributes.ChildContainer.Tag);

                if (attributes.ChildContainer.ClassNames != null && attributes.ChildContainer.ClassNames.Length > 0)
                    layout.NestedClassNames(attributes.ChildContainer.ClassNames);
            }

            if (attributes.UsesRegions != null)
                foreach (var usesRegion in attributes.UsesRegions)
                    layout.Region(usesRegion.RegionName, usesRegion.RegionElement);
    
            layout.Build();
        }

        private void BuildRegion(AttributeSet attributes)
        {
            var region = Region()
                .Name(attributes.IsRegion.Name);

            if (_packageContext != null)
                region.PartOf(_packageContext);

            if (attributes.DeployedAs != null)
                region.AssetDeployment(attributes.DeployedAs.Deployment)
                    .DeployIn(attributes.DeployedAs.ModuleName);

            if (attributes.NeedsDatas != null)
            {
                foreach (var needsData in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(needsData.DataContextName))
                        region.DataContext(needsData.DataContextName);

                    if (needsData.DataType != null)
                        region.BindTo(needsData.DataType);
                }
            }

            if (attributes.PartOf != null)
                region.PartOf(attributes.PartOf.PackageName);

            if (attributes.Style != null)
            {
                if (!string.IsNullOrEmpty(attributes.Style.CssStyle))
                    region.Style(attributes.Style.CssStyle);
            }

            if (attributes.Repeat != null)
                region.ForEach(
                    attributes.Repeat.ItemType, 
                    attributes.Repeat.Tag, 
                    attributes.Repeat.Style, 
                    attributes.Repeat.ClassNames);

            if (attributes.Container != null)
            {
                if (!string.IsNullOrEmpty(attributes.Container.Tag))
                    region.Tag(attributes.Container.Tag);

                if (attributes.Container.ClassNames != null && attributes.Container.ClassNames.Length > 0)
                    region.ClassNames(attributes.Container.ClassNames);
            }

            if (attributes.UsesLayouts != null)
                foreach(var usesLayout in attributes.UsesLayouts)
                    region.Layout(usesLayout.LayoutName);

            if (attributes.UsesComponents != null)
                foreach(var usesComponent in attributes.UsesComponents)
                    region.Component(usesComponent.ComponentName);

            region.Build();
        }

        private void BuildComponent(AttributeSet attributes)
        {
            var component = Component()
                .Name(attributes.IsComponent.Name);

            if (_packageContext != null)
                component.PartOf(_packageContext);

            if (attributes.DeployedAs != null)
                component.AssetDeployment(attributes.DeployedAs.Deployment)
                    .DeployIn(attributes.DeployedAs.ModuleName);

            if (attributes.PartOf != null)
                component.PartOf(attributes.PartOf.PackageName);

            if (attributes.NeedsDatas != null)
            {
                foreach (var needsData in attributes.NeedsDatas)
                {
                    if (!string.IsNullOrEmpty(needsData.DataContextName))
                        component.DataContext(needsData.DataContextName);

                    if (needsData.DataType != null)
                        component.BindTo(needsData.DataType);
                }
            }

            if (attributes.RenderHtmls != null)
            {
                foreach(var renderHtml in attributes.RenderHtmls)
                {
                    component.Render(renderHtml.TextName, renderHtml.Html);
                }
            }

            if (attributes.DeployCsss != null)
                foreach(var deployCss in attributes.DeployCsss)
                    component.DeployCss(deployCss.CssSelector, deployCss.CssStyle);

            if (attributes.DeployFunction != null)
                component.DeployFunction(
                    attributes.DeployFunction.ReturnType,
                    attributes.DeployFunction.FunctionName,
                    attributes.DeployFunction.Parameters,
                    attributes.DeployFunction.Body);

            component.Build();
        }

        private void BuildService(AttributeSet attributes)
        {
            var service = Service(attributes.Type)
                .Name(attributes.IsService.Name);

            service.Build();
        }

    }
}
