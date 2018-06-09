using System;
using System.Collections.Generic;
using System.Reflection;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Facilities.Builders
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

        private readonly HashSet<string> _assemblies = new HashSet<string>();
        private readonly HashSet<string> _types = new HashSet<string>();

        void IFluentBuilder.Register(IPackage package)
        {
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

            //package.Build(this);
        }

        void IFluentBuilder.Register(Assembly assembly)
        {
            if (!_assemblies.Add(assembly.FullName))
                return;

            var types = assembly.GetTypes();

            Exception exception = null;

            foreach (var type in types)
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

            IsModuleAttribute isModule = null;
            IsPageAttribute isPage = null;
            IsLayoutAttribute isLayout = null;
            IsRegionAttribute isRegion = null;
            IsComponentAttribute isComponent = null;
            IsServiceAttribute isService = null;

            foreach (var attribute in attributes)
            {
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

            if (isModule != null) BuildModule(type, isModule, attributes);
            if (isPage != null) BuildPage(type, isPage, attributes);
            if (isLayout != null) BuildLayout(type, isLayout, attributes);
            if (isRegion != null) BuildRegion(type, isRegion, attributes);
            if (isComponent != null) BuildComponent(type, isComponent, attributes);
            if (isService != null) BuildService(type, isService, attributes);
        }

        private void BuildModule(Type type, IsModuleAttribute isModule, object[] attributes)
        {
            if (ModuleBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build modules");
        }

        private void BuildPage(Type type, IsPageAttribute isPage, object[] attributes)
        {
            if (PageBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build pages");

            var page = PageBuilder.Page(type).Name(isPage.Name);

            foreach (var attribute in attributes)
            {
                var deployedAs = attribute as DeployedAsAttribute;
                var hasLayout = attribute as UsesLayoutAttribute;
                var partOf = attribute as PartOfAttribute;
                var regionComponent = attribute as RegionComponentAttribute;
                var regionLayout = attribute as RegionLayoutAttribute;
                var route = attribute as RouteAttribute;
                var title = attribute as PageTitleAttribute;

                if (deployedAs != null)
                    page.AssetDeployment(deployedAs.Deployment);

                if (hasLayout != null)
                    page.Layout(hasLayout.LayoutName);

                if (partOf != null)
                    page.Module(partOf.PackageName);

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
            }

            page.Build();
        }

        private void BuildLayout(Type type, IsLayoutAttribute isLayout, object[] attributes)
        {
            if (LayoutBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build layouts");
        }

        private void BuildRegion(Type type, IsRegionAttribute isRegion, object[] attributes)
        {
            if (RegionBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build regions");
        }

        private void BuildComponent(Type type, IsComponentAttribute isComponent, object[] attributes)
        {
            if (ComponentBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build components");
        }

        private void BuildService(Type type, IsServiceAttribute isPage, object[] attributes)
        {
            if (ServiceBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build pages");
        }

        IComponentDefinition IComponentBuilder.Component()
        {
            if (ComponentBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build components");
            return ComponentBuilder.Component();
        }

        IRegionDefinition IRegionBuilder.Region()
        {
            if (RegionBuilder == null)
                throw new BuilderException("There is no build engine installed that knows how to build regions");
            return RegionBuilder.Region();
        }

        ILayoutDefinition ILayoutBuilder.Layout()
        {
            throw new NotImplementedException();
        }

        IPageDefinition IPageBuilder.Page(Type declaringType)
        {
            throw new NotImplementedException();
        }
    }
}
