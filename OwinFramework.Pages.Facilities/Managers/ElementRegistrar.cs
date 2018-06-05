using System;
using System.Collections.Generic;
using System.Reflection;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Facilities.Managers
{
    /// <summary>
    /// Uses reflection to find pages, layouts, regions, components
    /// and services in the solution and registers then and wires
    /// them together
    /// </summary>
    internal class ElementRegistrar : IElementRegistrar
    {
        private readonly IPageBuilder _pageBuilder;

        private readonly HashSet<string> _assemblies = new HashSet<string>();
        private readonly HashSet<string> _types = new HashSet<string>();

        /// <summary>
        /// IoC injection constructor
        /// </summary>
        public ElementRegistrar(
            IPageBuilder pageBuilder)
        {
            _pageBuilder = pageBuilder;
        }

        void IElementRegistrar.Register(IPackage package)
        {
        }

        void IElementRegistrar.Register(Assembly assembly)
        {
            if (!_assemblies.Add(assembly.FullName))
                return;

            var types = assembly.GetTypes();

            Exception exception = null;

            foreach (var type in types)
            {
                try
                {
                    ((IElementRegistrar)this).Register(type);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            if (exception != null)
                throw exception;
        }

        void IElementRegistrar.Register(Type type)
        {
            if (!_types.Add(type.FullName))
                return;

            var attributes = type.GetCustomAttributes(true);

            IsPageAttribute isPage = null;
            IsServiceAttribute isService = null;

            foreach (var attribute in attributes)
            {
                if (attribute is IsPageAttribute)
                    isPage = attribute as IsPageAttribute;

                if (attribute is IsServiceAttribute)
                    isService = attribute as IsServiceAttribute;
            }

            if (isPage != null) BuildPage(type, isPage, attributes);
            if (isService != null) BuildService(type, isService, attributes);
        }

        private void BuildPage(Type type, IsPageAttribute isPage, object[] attributes)
        {
            var page = _pageBuilder.Page(type).Name(isPage.Name);

            foreach (var attribute in attributes)
            {
                var deployedAs = attribute as DeployedAsAttribute;
                var hasLayout = attribute as HasLayoutAttribute;
                var partOf = attribute as PartOfAttribute;
                var regionComponent = attribute as RegionComponentAttribute;
                var regionLayout = attribute as RegionLayoutAttribute;
                var route = attribute as RouteAttribute;

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
            }

            page.Build();
        }

        private void BuildService(Type type, IsServiceAttribute isPage, object[] attributes)
        {
        }
    }
}
