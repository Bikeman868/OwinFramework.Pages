using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Encapsulates the attributes that can be attached to classes
    /// to define them as elements of the page
    /// </summary>
    public class AttributeSet
    {
        /// <summary>
        /// This is the type whos attributes are captured here
        /// </summary>
        public Type Type;

        /// <summary>
        /// Defines this class as a package
        /// </summary>
        public IsPackageAttribute IsPackage;

        /// <summary>
        /// Defines this class as a module
        /// </summary>
        public IsModuleAttribute IsModule;

        /// <summary>
        /// Defines this class as a page
        /// </summary>
        public IsPageAttribute IsPage;

        /// <summary>
        /// Defines this class as a layout
        /// </summary>
        public IsLayoutAttribute IsLayout;

        /// <summary>
        /// Defines this class as a region
        /// </summary>
        public IsRegionAttribute IsRegion;

        /// <summary>
        /// Defines this class as a component
        /// </summary>
        public IsComponentAttribute IsComponent;

        /// <summary>
        /// Defines this class as a service
        /// </summary>
        public IsServiceAttribute IsService;

        /// <summary>
        /// Defines this class as a data provider
        /// </summary>
        public IsDataProviderAttribute IsDataProvider;

        /// <summary>
        /// Specifies how child containers should be handled
        /// </summary>
        public ChildContainerAttribute ChildContainer;

        /// <summary>
        /// Specifies how child elements should be styled
        /// </summary>
        public ChildStyleAttribute ChildStyle;

        /// <summary>
        /// Specifies how the main containers should behave
        /// </summary>
        public ContainerAttribute Container;

        /// <summary>
        /// Specifies how the JavaScript and css assets for this 
        /// element should be deployed to the browser
        /// </summary>
        public DeployedAsAttribute DeployedAs;

        /// <summary>
        /// Adds static text to the title of the page
        /// </summary>
        public PageTitleAttribute PageTitle;

        /// <summary>
        /// Defines this element to be part of a package. The package
        /// defines the namespace to use for JavaScript and css
        /// </summary>
        public PartOfAttribute PartOf;

        /// <summary>
        /// Specifies that this element should bind to a list of data
        /// and repeat its contents for each item on the list
        /// </summary>
        public RepeatAttribute Repeat;

        /// <summary>
        /// Specifies a permission that the user must have assigned
        /// to them to be able to access this element
        /// </summary>
        public RequiresPermissionAttribute RequiresPermission;

        /// <summary>
        /// Specifies that the caller must identify themselves to the
        /// system in order to call this endpoint
        /// </summary>
        public RequiresIdentificationAttribute RequiresIdentification;

        /// <summary>
        /// Defines how to style this element
        /// </summary>
        public StyleAttribute Style;

        /// <summary>
        /// Defines rules for caching the output from this runable
        /// </summary>
        public CacheOutputAttribute CacheOutput;

        /// <summary>
        /// For a region, specifies the layout to render inside the region
        /// </summary>
        public UsesLayoutAttribute UsesLayout;

        /// <summary>
        /// Specifies the templates to render for this element
        /// </summary>
        public IList<RenderTemplateAttribute> RenderTemplates;

        /// <summary>
        /// Defines a static css asset to deploy on any page that includes this element
        /// </summary>
        public IList<DeployCssAttribute> DeployCsss;

        /// <summary>
        /// A list of JavaScriot functions to deploy
        /// </summary>
        public IList<DeployFunctionAttribute> DeployFunctions;

        /// <summary>
        /// Defines a component that must be present on any page that includes this element
        /// </summary>
        public IList<NeedsComponentAttribute> NeedsComponents;

        /// <summary>
        /// Specifies that this element needs specific data to be available at page rendering time
        /// </summary>
        public IList<NeedsDataAttribute> NeedsDatas;

        /// <summary>
        /// For a layout, specifies a component to place in one of the regions of the layout
        /// </summary>
        public IList<RegionComponentAttribute> RegionComponents;

        /// <summary>
        /// For a layout, specifies a layout to place in one of the regions of the layout
        /// </summary>
        public IList<RegionLayoutAttribute> RegionLayouts;

        /// <summary>
        /// For a layout, specifies html to place in one of the regions of the layout
        /// </summary>
        public IList<RegionHtmlAttribute> RegionHtmls;

        /// <summary>
        /// For a layout, specifies html to place in one of the regions of the layout
        /// </summary>
        public IList<RegionTemplateAttribute> RegionTemplates;

        /// <summary>
        /// Defines some static html to render into the component or region
        /// </summary>
        public IList<RenderHtmlAttribute> RenderHtmls;

        /// <summary>
        /// Defines URLs that should be routed to this page or service
        /// </summary>
        public IList<RouteAttribute> Routes;

        /// <summary>
        /// For a region, specifies the component to render inside the region
        /// </summary>
        public IList<UsesComponentAttribute> UsesComponents;

        /// <summary>
        /// For a layout, defines the region component to use for each region of the layout
        /// </summary>
        public IList<LayoutRegionAttribute> LayoutRegions;

        /// <summary>
        /// For a region, defines a type of data that will be resolved within this region
        /// </summary>
        public IList<DataScopeAttribute> DataScopes;

        /// <summary>
        /// For a data provider, defines a type of data that it can supply
        /// </summary>
        public IList<SuppliesDataAttribute> SuppliesDatas;

        /// <summary>
        /// Constructs a new attribute set for a class type
        /// </summary>
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

                Container = Set(Container, attribute);
                ChildContainer = Set(ChildContainer, attribute);
                ChildStyle = Set(ChildStyle, attribute);
                DeployedAs = Set(DeployedAs, attribute);
                PageTitle = Set(PageTitle, attribute);
                PartOf = Set(PartOf, attribute);
                Repeat = Set(Repeat, attribute);
                RequiresPermission = Set(RequiresPermission, attribute);
                RequiresIdentification = Set(RequiresIdentification, attribute);
                Style = Set(Style, attribute);
                CacheOutput = Set(CacheOutput, attribute);
                UsesLayout = Set(UsesLayout, attribute);

                DeployCsss = Add(DeployCsss, attribute);
                DeployFunctions = Add(DeployFunctions, attribute);
                NeedsComponents = Add(NeedsComponents, attribute);
                NeedsDatas = Add(NeedsDatas, attribute);
                RegionComponents = Add(RegionComponents, attribute);
                RegionLayouts = Add(RegionLayouts, attribute);
                RegionHtmls = Add(RegionHtmls, attribute);
                RegionTemplates = Add(RegionTemplates, attribute);
                RenderHtmls = Add(RenderHtmls, attribute);
                RenderTemplates = Add(RenderTemplates, attribute);
                Routes = Add(Routes, attribute);
                UsesComponents = Add(UsesComponents, attribute);
                LayoutRegions = Add(LayoutRegions, attribute);
                DataScopes = Add(DataScopes, attribute);
                SuppliesDatas = Add(SuppliesDatas, attribute);
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

        /// <summary>
        /// Checks these attributes and reports any attributes that are missplaced
        /// </summary>
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
