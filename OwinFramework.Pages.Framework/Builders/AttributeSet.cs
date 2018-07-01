using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;

namespace OwinFramework.Pages.Framework.Builders
{
    internal class AttributeSet
    {
        public Type Type;

        // Attributes that define the type of element
        public IsPackageAttribute IsPackage;
        public IsModuleAttribute IsModule;
        public IsPageAttribute IsPage;
        public IsLayoutAttribute IsLayout;
        public IsRegionAttribute IsRegion;
        public IsComponentAttribute IsComponent;
        public IsServiceAttribute IsService;
        public IsDataProviderAttribute IsDataProvider;

        // Attributes that can only be applied once
        public ChildContainerAttribute ChildContainer;
        public ChildStyleAttribute ChildStyle;
        public ContainerAttribute Container;
        public DeployFunctionAttribute DeployFunction;
        public DeployedAsAttribute DeployedAs;
        public PageTitleAttribute PageTitle;
        public PartOfAttribute PartOf;
        public RepeatAttribute Repeat;
        public RequiresPermissionAttribute RequiresPermission;
        public StyleAttribute Style;

        // Attributes that can be applied multiple times
        public IList<DeployCssAttribute> DeployCsss;
        public IList<NeedsComponentAttribute> NeedsComponents;
        public IList<NeedsDataAttribute> NeedsDatas;
        public IList<RegionComponentAttribute> RegionComponents;
        public IList<RegionLayoutAttribute> RegionLayouts;
        public IList<RenderHtmlAttribute> RenderHtmls;
        public IList<RouteAttribute> Routes;
        public IList<UsesComponentAttribute> UsesComponents;
        public IList<UsesLayoutAttribute> UsesLayouts;
        public IList<UsesRegionAttribute> UsesRegions;

        public AttributeSet(Type type)
        {
            Type = type;
            var attributes = type.GetCustomAttributes(true);

            foreach (var attribute in attributes)
            {
                IsPackage = Set(IsPackage, attribute);
                IsModule = Set(IsModule, attribute);
                IsPage = Set(IsPage, attribute);
                IsLayout = Set(IsLayout, attribute);
                IsRegion = Set(IsRegion, attribute);
                IsComponent = Set(IsComponent, attribute);
                IsService = Set(IsService, attribute);
                IsDataProvider = Set(IsDataProvider, attribute);

                ChildContainer = Set(ChildContainer, attribute);
                ChildStyle = Set(ChildStyle, attribute);
                DeployFunction = Set(DeployFunction, attribute);
                DeployedAs = Set(DeployedAs, attribute);
                PageTitle = Set(PageTitle, attribute);
                PartOf = Set(PartOf, attribute);
                Repeat = Set(Repeat, attribute);
                RequiresPermission = Set(RequiresPermission, attribute);
                Style = Set(Style, attribute);

                DeployCsss = Add(DeployCsss, attribute);
                NeedsComponents = Add(NeedsComponents, attribute);
                NeedsDatas = Add(NeedsDatas, attribute);
                RegionComponents = Add(RegionComponents, attribute);
                RegionLayouts = Add(RegionLayouts, attribute);
                RenderHtmls = Add(RenderHtmls, attribute);
                Routes = Add(Routes, attribute);
                UsesComponents = Add(UsesComponents, attribute);
                UsesLayouts = Add(UsesLayouts, attribute);
                UsesRegions = Add(UsesRegions, attribute);
            }
        }

        private T Set<T>(T existing, object attribute) where T: Attribute
        { 
            if (attribute is T)
            {
                if (existing != null)
                    throw new FluentBuilderException("You can only add the " + typeof(T).Name + " attribute once to " + Type.DisplayName());
                return (T)attribute;
            }
            return existing;
        }

        private IList<T> Add<T>(IList<T> list, object attribute) where T: Attribute
        {
            if (attribute is T)
            {
                if (list == null) list = new List<T>();
                list.Add((T)attribute);
            }
            return list;
        }

        public void Validate()
        {
            if (IsPackage != null) ValidatePackage();
            else if (IsModule != null) ValidateModule();
            else if (IsPage != null) ValidatePage();
            else if (IsLayout != null) ValidateLayout();
            else if (IsRegion != null) ValidateRegion();
            else if (IsComponent != null) ValidateComponent();
            else if (IsService != null) ValidateService();
            else if (IsDataProvider != null) ValidateDataProvider();
        }

        private void ValidatePackage()
        {
            if (IsModule != null) throw new FluentBuilderException("A class can not be a package and a module. " + Type.DisplayName());
            if (IsPage != null) throw new FluentBuilderException("A class can not be a package and a page. " + Type.DisplayName());
            if (IsLayout != null) throw new FluentBuilderException("A class can not be a package and a layout. " + Type.DisplayName());
            if (IsRegion != null) throw new FluentBuilderException("A class can not be a package and a region. " + Type.DisplayName());
            if (IsComponent != null) throw new FluentBuilderException("A class can not be a package and a component. " + Type.DisplayName());
            if (IsService != null) throw new FluentBuilderException("A class can not be a package and a service. " + Type.DisplayName());
            if (IsDataProvider != null) throw new FluentBuilderException("A class can not be a package and a data provider. " + Type.DisplayName());
        }

        private void ValidateModule()
        {
            if (IsPackage != null) throw new FluentBuilderException("A class can not be a module and a package. " + Type.DisplayName());
            if (IsPage != null) throw new FluentBuilderException("A class can not be a module and a page. " + Type.DisplayName());
            if (IsLayout != null) throw new FluentBuilderException("A class can not be a module and a layout. " + Type.DisplayName());
            if (IsRegion != null) throw new FluentBuilderException("A class can not be a module and a region. " + Type.DisplayName());
            if (IsComponent != null) throw new FluentBuilderException("A class can not be a module and a component. " + Type.DisplayName());
            if (IsService != null) throw new FluentBuilderException("A class can not be a module and a service. " + Type.DisplayName());
        }

        private void ValidatePage()
        {
            if (IsPackage != null) throw new FluentBuilderException("A class can not be a page and a package. " + Type.DisplayName());
            if (IsModule != null) throw new FluentBuilderException("A class can not be a page and a module. " + Type.DisplayName());
            if (IsLayout != null) throw new FluentBuilderException("A class can not be a page and a layout. " + Type.DisplayName());
            if (IsRegion != null) throw new FluentBuilderException("A class can not be a page and a region. " + Type.DisplayName());
            if (IsComponent != null) throw new FluentBuilderException("A class can not be a page and a component. " + Type.DisplayName());
            if (IsService != null) throw new FluentBuilderException("A class can not be a page and a service. " + Type.DisplayName());
            if (IsDataProvider != null) throw new FluentBuilderException("A class can not be a page and a data provider. " + Type.DisplayName());
        }

        private void ValidateLayout()
        {
            if (IsPackage != null) throw new FluentBuilderException("A class can not be a layout and a package. " + Type.DisplayName());
            if (IsModule != null) throw new FluentBuilderException("A class can not be a layout and a module. " + Type.DisplayName());
            if (IsPage != null) throw new FluentBuilderException("A class can not be a layout and a page. " + Type.DisplayName());
            if (IsRegion != null) throw new FluentBuilderException("A class can not be a layout and a region. " + Type.DisplayName());
            if (IsComponent != null) throw new FluentBuilderException("A class can not be a layout and a component. " + Type.DisplayName());
            if (IsService != null) throw new FluentBuilderException("A class can not be a layout and a service. " + Type.DisplayName());
            if (IsDataProvider != null) throw new FluentBuilderException("A class can not be a layout and a data provider. " + Type.DisplayName());
        }

        private void ValidateRegion()
        {
            if (IsPackage != null) throw new FluentBuilderException("A class can not be a region and a package. " + Type.DisplayName());
            if (IsModule != null) throw new FluentBuilderException("A class can not be a region and a module. " + Type.DisplayName());
            if (IsPage != null) throw new FluentBuilderException("A class can not be a region and a page. " + Type.DisplayName());
            if (IsLayout != null) throw new FluentBuilderException("A class can not be a region and a layout. " + Type.DisplayName());
            if (IsComponent != null) throw new FluentBuilderException("A class can not be a region and a component. " + Type.DisplayName());
            if (IsService != null) throw new FluentBuilderException("A class can not be a region and a service. " + Type.DisplayName());
            if (IsDataProvider != null) throw new FluentBuilderException("A class can not be a region and a data provider. " + Type.DisplayName());
        }

        private void ValidateComponent()
        {
            if (IsPackage != null) throw new FluentBuilderException("A class can not be a component and a package. " + Type.DisplayName());
            if (IsModule != null) throw new FluentBuilderException("A class can not be a component and a module. " + Type.DisplayName());
            if (IsPage != null) throw new FluentBuilderException("A class can not be a component and a page. " + Type.DisplayName());
            if (IsLayout != null) throw new FluentBuilderException("A class can not be a component and a layout. " + Type.DisplayName());
            if (IsRegion != null) throw new FluentBuilderException("A class can not be a component and a region. " + Type.DisplayName());
            if (IsService != null) throw new FluentBuilderException("A class can not be a component and a service. " + Type.DisplayName());
            if (IsDataProvider != null) throw new FluentBuilderException("A class can not be a component and a data provider. " + Type.DisplayName());
        }

        private void ValidateService()
        {
            if (IsPackage != null) throw new FluentBuilderException("A class can not be a service and a package. " + Type.DisplayName());
            if (IsModule != null) throw new FluentBuilderException("A class can not be a service and a module. " + Type.DisplayName());
            if (IsPage != null) throw new FluentBuilderException("A class can not be a service and a page. " + Type.DisplayName());
            if (IsLayout != null) throw new FluentBuilderException("A class can not be a service and a layout. " + Type.DisplayName());
            if (IsRegion != null) throw new FluentBuilderException("A class can not be a service and a region. " + Type.DisplayName());
            if (IsComponent != null) throw new FluentBuilderException("A class can not be a service and a component. " + Type.DisplayName());
            if (IsDataProvider != null) throw new FluentBuilderException("A class can not be a service and a data provider. " + Type.DisplayName());
        }

        private void ValidateDataProvider()
        {
            if (IsPackage != null) throw new FluentBuilderException("A class can not be a data provider and a package. " + Type.DisplayName());
            if (IsModule != null) throw new FluentBuilderException("A class can not be a data provider and a module. " + Type.DisplayName());
            if (IsPage != null) throw new FluentBuilderException("A class can not be a data provider and a page. " + Type.DisplayName());
            if (IsLayout != null) throw new FluentBuilderException("A class can not be a data provider and a layout. " + Type.DisplayName());
            if (IsRegion != null) throw new FluentBuilderException("A class can not be a data provider and a region. " + Type.DisplayName());
            if (IsComponent != null) throw new FluentBuilderException("A class can not be a data provider and a component. " + Type.DisplayName());
            if (IsService != null) throw new FluentBuilderException("A class can not be a data provider and a service. " + Type.DisplayName());
        }
    }
}
