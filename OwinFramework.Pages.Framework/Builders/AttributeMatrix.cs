using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Framework.Builders
{
    internal class AttributeMatrix
    {
        private readonly Dictionary<Type, AttributeDefinition> _attributeDefinitions;

        private class AttributeUsage
        {
            public Type DefinitionType { get; private set; }
            public bool IsValid { get; private set; }
            public string Message { get; private set; }

            public AttributeUsage(Type definitionType)
            {
                DefinitionType = definitionType;
                IsValid = true;
            }

            public AttributeUsage(Type definitionType, string message)
            {
                DefinitionType = definitionType;
                IsValid = false;
                Message = message;
            }
        }

        private class AttributeDefinition
        {
            private readonly Type _attributeType;
            private readonly List<AttributeUsage> _usage;
            private readonly string _usageMessage;

            public AttributeDefinition(Type attributeType, string usageMessage = null)
            {
                _attributeType = attributeType;
                _usageMessage = usageMessage ?? string.Empty;
                _usage = new List<AttributeUsage>();
            }

            public AttributeDefinition Valid<T>()
            {
                _usage.Add(new AttributeUsage(typeof(T)));
                return this;
            }

            public AttributeDefinition Valid(params Type[] definitionTypes)
            {
                _usage.AddRange(definitionTypes.Select(t => new AttributeUsage(t)));
                return this;
            }

            public AttributeDefinition Invalid<T>(string message)
            {
                _usage.Add(new AttributeUsage(typeof(T), message));
                return this;
            }

            public string Check<T>()
            {
                var type = typeof(T);
                var usage = _usage.FirstOrDefault(u => u.DefinitionType == type);

                if (usage != null && usage.IsValid) return null;

                var result = 
                    "Attribute '" + _attributeType.DisplayName(TypeExtensions.NamespaceOption.None) +
                    "' is not valid for '" + type.DisplayName(TypeExtensions.NamespaceOption.None) + "'.";

                if (usage != null && !String.IsNullOrEmpty(usage.Message))
                    result += " " + usage.Message;
                else if (!string.IsNullOrEmpty(_usageMessage))
                    result += " " + _usageMessage;

                return result;
            }
        }

        public AttributeMatrix()
        {
            _attributeDefinitions = new Dictionary<Type, AttributeDefinition>();

            Add<ChildContainerAttribute>("The [ChildContainerAttribute] only applies to elements that have other elements within them. To change the html element for this element add the [Container] attribute.")
                .Valid<ILayoutDefinition>()
                .Valid<ILayout>()
                .Invalid<IPageDefinition>("Pages have a layout that defines the container.")
                .Invalid<IRegionDefinition>("Regions define their child container in the [Repeat] attribute.");

            Add<ChildStyleAttribute>("The [ChildStyleAttribute] only applies to elements that have other elements within them.")
                .Valid<ILayoutDefinition>()
                .Invalid<IRegionDefinition>("Regions can not style their contents because any component or layout can reside in the region.")
                .Invalid<IPageDefinition>("Pages have a layout that defines the style of the content.");

            Add<ContainerAttribute>("Only layouts and regions wrap their children in HTML container elements.")
                .Valid<ILayoutDefinition>()
                .Valid<IRegionDefinition>()
                .Invalid<IComponentDefinition>("Components do not have containers, the component is responsible for rendering its Html")
                .Invalid<IPageDefinition>("The container for a page is always the <body> tag, changing to some other tag would break the html.");

            Add<DataScopeAttribute>("")
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>()
                .Invalid<IComponentDefinition>("You can only introduce a data scope for a region, page or service.")
                .Invalid<ILayoutDefinition>("You can only introduce a data scope for a region, page or service.");

            Add<DeployCssAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>();

            Add<DeployFunctionAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>();

            Add<DeployedAsAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<ILayoutDefinition>()
                .Valid<IServiceDefinition>()
                .Valid<IPackageDefinition>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>()
                .Invalid<IModuleDefinition>("Modules can not contain other modules.");

            Add<NeedsComponentAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>()
                .Invalid<IModuleDefinition>("Only elements that render Html can have component library dependencies.")
                .Invalid<IPackageDefinition>("Only elements that render Html can have component library dependencies.")
                .Invalid<IDataProviderDefinition>("Only elements that render Html can have component library dependencies.");

            Add<NeedsDataAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<IDataProviderDefinition>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>();

            Add<PageTitleAttribute>("The [PageTitle] attribute can only be applied to pages.")
                .Valid<IPageDefinition>();

            Add<PartOfAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<IDataProviderDefinition>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>()
                .Valid<IServiceDefinition>()
                .Invalid<IModuleDefinition>("Modules and Packages are distinctly different ways of grouping elements, you can not put a module into a package.")
                .Invalid<IPackageDefinition>("Packages can not contain other packages.");

            Add<ZoneComponentAttribute>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<ILayout>()
                .Invalid<IRegionDefinition>("Regions only contain a single element, only layouts have zones");
            
            Add<ZoneHtmlAttribute>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Invalid<IRegionDefinition>("Regions only contain a single element, only layouts have zones");

            Add<ZoneTemplateAttribute>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Invalid<IRegionDefinition>("Regions only contain a single element, only layouts have zones");

            Add<ZoneLayoutAttribute>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<ILayout>()
                .Invalid<IRegionDefinition>("Regions only contain a single element, only layouts have zones");

            Add<RenderHtmlAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<IRegionDefinition>()
                .Invalid<ILayoutDefinition>("Components render Html, layouts are used to define the structure of the page.");

            Add<RepeatAttribute>()
                .Valid<IRegionDefinition>()
                .Invalid<ILayoutDefinition>("Use a region to repeat the content for each item on a data-bound list.")
                .Invalid<IComponentDefinition>("Use a region to repeat the content for each item on a data-bound list.")
                .Invalid<IPageDefinition>("Use a region to repeat the content for each item on a data-bound list.");

            Add<RequiresPermissionAttribute>()
                .Valid<IPageDefinition>()
                .Valid<IServiceDefinition>()
                .Valid<IPage>()
                .Valid<IService>();

            Add<RequiresIdentificationAttribute>()
                .Valid<IPageDefinition>()
                .Valid<IServiceDefinition>()
                .Valid<IPage>()
                .Valid<IService>();

            Add<RouteAttribute>("Only pages and services can handle Http requests and return responses. Elements like regions, layouts and components produce fragments of Html not entire responses.")
                .Valid<IPageDefinition>()
                .Valid<IServiceDefinition>();

            Add<StyleAttribute>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>()
                .Invalid<IComponentDefinition>("Components do not render any Html elements that this style could be applied to. Specify the style within the Html that is rendered by the component.");

            Add<CacheOutputAttribute>("You can only define caching for elements that produce complete responses to Http requests.")
                .Valid<IPageDefinition>()
                .Valid<IServiceDefinition>()
                .Valid<IPage>()
                .Valid<IService>();

            Add<SuppliesDataAttribute>()
                .Valid<IDataProviderDefinition>();

            Add<UsesComponentAttribute>()
                .Valid<IRegionDefinition>()
                .Valid<IRegion>()
                .Invalid<ILayoutDefinition>("Please use the [ZoneComponent] attribute instead so that the zone name can be specified.")
                .Invalid<IPageDefinition>("You can not add components directly to the page but you can use the [NeedsComponent] attribute to add component dependencies.");

            Add<UsesLayoutAttribute>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>()
                .Valid<IRegion>()
                .Invalid<ILayoutDefinition>("Please use the [ZoneLayout] attribute instead so that the zone name can be specified.");

            Add<RenderTemplateAttribute>()
                .Valid<IRegionDefinition>()
                .Invalid<IPageDefinition>("Please use the [ZoneTemplate] attribute instead so that the zone name can be specified.")
                .Invalid<ILayoutDefinition>("Please use the [ZoneTemplate] attribute instead so that the zone name can be specified.");

            Add<ZoneRegionAttribute>()
                .Valid<ILayoutDefinition>()
                .Invalid<IPageDefinition>("Pages can only directly contain layouts. The layout defines the regions. Pages can override the contents of the layout regions using [ZoneTemplate], [ZoneLayout] and [ZoneComponent] attributes.");

            Add<GenerateClientScriptAttribute>("You can only add this attribute to service definitions. It instructs the service builder to generate client-side JavaScript for the service.")
                .Valid<IServiceDefinition>();
        }

        private AttributeDefinition Add<T>(string usageMessage = null)
        {
            var type = typeof(T);
            var definition = new AttributeDefinition(type, usageMessage);
            _attributeDefinitions[type] = definition;
            return definition;
        }

        public List<string> Validate<T>(AttributeSet attributes)
        {
            var result = new List<string>();

            if (attributes.ChildContainer != null) CheckAttribute<T, ChildContainerAttribute>(result);
            if (attributes.ChildStyle != null) CheckAttribute<T, ChildStyleAttribute>(result);
            if (attributes.Container != null) CheckAttribute<T, ContainerAttribute>(result);
            if (attributes.DataScopes != null) CheckAttribute<T, DataScopeAttribute>(result);
            if (attributes.DeployCsss != null) CheckAttribute<T, DeployCssAttribute>(result);
            if (attributes.DeployFunctions != null) CheckAttribute<T, DeployFunctionAttribute>(result);
            if (attributes.DeployedAs != null) CheckAttribute<T, DeployedAsAttribute>(result);
            if (attributes.NeedsComponents != null) CheckAttribute<T, NeedsComponentAttribute>(result);
            if (attributes.NeedsDatas != null) CheckAttribute<T, NeedsDataAttribute>(result);
            if (attributes.PageTitle != null) CheckAttribute<T, PageTitleAttribute>(result);
            if (attributes.PartOf != null) CheckAttribute<T, PartOfAttribute>(result);
            if (attributes.RegionComponents != null) CheckAttribute<T, ZoneComponentAttribute>(result);
            if (attributes.RegionLayouts != null) CheckAttribute<T, ZoneLayoutAttribute>(result);
            if (attributes.RegionHtmls != null) CheckAttribute<T, ZoneHtmlAttribute>(result);
            if (attributes.RegionTemplates != null) CheckAttribute<T, ZoneTemplateAttribute>(result);
            if (attributes.RenderHtmls != null) CheckAttribute<T, RenderHtmlAttribute>(result);
            if (attributes.Repeat != null) CheckAttribute<T, RepeatAttribute>(result);
            if (attributes.RequiresPermission != null) CheckAttribute<T, RequiresPermissionAttribute>(result);
            if (attributes.RequiresIdentification != null) CheckAttribute<T, RequiresIdentificationAttribute>(result);
            if (attributes.Routes != null) CheckAttribute<T, RouteAttribute>(result);
            if (attributes.Style != null) CheckAttribute<T, StyleAttribute>(result);
            if (attributes.SuppliesDatas != null) CheckAttribute<T, SuppliesDataAttribute>(result);
            if (attributes.UsesComponents != null) CheckAttribute<T, UsesComponentAttribute>(result);
            if (attributes.UsesLayout != null) CheckAttribute<T, UsesLayoutAttribute>(result);
            if (attributes.RenderTemplates != null) CheckAttribute<T, RenderTemplateAttribute>(result);
            if (attributes.LayoutRegions != null) CheckAttribute<T, ZoneRegionAttribute>(result);
            if (attributes.GenerateClientScript != null) CheckAttribute<T, GenerateClientScriptAttribute>(result);

            return result.Count > 0 ? result : null;
        }

        private void CheckAttribute<TElement, TAttribute>(ICollection<string> result)
        {
            var definition = _attributeDefinitions[typeof(TAttribute)];
            var message = definition.Check<TElement>();
            if (!string.IsNullOrEmpty(message))
                result.Add(message);
        }
    }
}