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

            public AttributeDefinition(Type attributeType)
            {
                _attributeType = attributeType;
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

                return result;
            }
        }

        public AttributeMatrix()
        {
            _attributeDefinitions = new Dictionary<Type, AttributeDefinition>();

            Add<ChildContainerAttribute>()
                .Valid<ILayoutDefinition>()
                .Valid<ILayout>()
                .Invalid<IComponentDefinition>("Components can not have children, they are the leaves of the visual tree.")
                .Invalid<IRegionDefinition>("Use the [Container] attribute instead.")
                .Invalid<IPageDefinition>("Pages have a layout that defines the container.");

            Add<ChildStyleAttribute>()
                .Valid<ILayoutDefinition>()
                .Invalid<IComponentDefinition>("Components can not have children, they are the leaves of the visual tree.")
                .Invalid<IRegionDefinition>("Regions can not style their contents because any component or layout can reside in the region.")
                .Invalid<IPageDefinition>("Pages have a layout that defines the style of the content.");

            Add<ContainerAttribute>()
                .Valid<ILayoutDefinition>()
                .Valid<IRegionDefinition>()
                .Invalid<IComponentDefinition>("Components do not have containers, the component is responsible for rendering its Html");

            Add<DataScopeAttribute>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>()
                .Valid<IServiceDefinition>()
                .Invalid<IComponentDefinition>("You can only introduce a data scope for a region, page or service.")
                .Invalid<ILayoutDefinition>("You can only introduce a data scope for a region, page or service.");

            Add<DeployCssAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<ILayoutDefinition>()
                .Invalid<IRegionDefinition>("Regions can not deploy CSS in this version."); // TODO: Regions should be able to deploy CSS

            Add<DeployFunctionAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<ILayoutDefinition>()
                .Invalid<IRegionDefinition>("Regions can not deploy JavaScript in this version."); // TODO: Regions should be able to deploy JavaScript

            Add<DeployedAsAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<ILayoutDefinition>()
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

            Add<PageTitleAttribute>()
                .Valid<IPageDefinition>()
                .Invalid<IComponentDefinition>("The [PageTitle] attribute can only be applied to pages.")
                .Invalid<ILayoutDefinition>("The [PageTitle] attribute can only be applied to pages.")
                .Invalid<IRegionDefinition>("The [PageTitle] attribute can only be applied to pages.");

            Add<PartOfAttribute>()
                .Valid<IComponentDefinition>()
                .Valid<IDataProviderDefinition>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>()
                .Valid<IServiceDefinition>()
                .Invalid<IModuleDefinition>("Modules and Packages are distinctly different ways of grouping elements, you can not put a module into a package.")
                .Invalid<IPackageDefinition>("Packages can not contain other packages.");

            Add<RegionComponentAttribute>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<ILayout>()
                .Invalid<IRegionDefinition>("Regions only contain a single element which can be either a layout or a component");

            Add<RegionLayoutAttribute>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<ILayout>()
                .Invalid<IRegionDefinition>("Regions only contain a single element which can be either a layout or a component");

            Add<RenderHtmlAttribute>()
                .Valid<IComponentDefinition>()
                .Invalid<ILayoutDefinition>("Components render Html, layouts are used to define the structure of the page.")
                .Invalid<IRegionDefinition>("Components render Html, regions are used to define the structure of the page, define data scopes and repeat content.");

            Add<RepeatAttribute>()
                .Valid<IRegionDefinition>()
                .Invalid<ILayoutDefinition>("Use a region to repeat the content for each item on a data-bound list.")
                .Invalid<IComponentDefinition>("Use a region to repeat the content for each item on a data-bound list.")
                .Invalid<IPageDefinition>("Use a region to repeat the content for each item on a data-bound list.");

            Add<RequiresPermissionAttribute>()
                .Invalid<IPageDefinition>("This feature is not implemented yet."); // TODO: Implement page based permissions

            Add<RouteAttribute>()
                .Valid<IPageDefinition>()
                .Valid<IServiceDefinition>()
                .Invalid<ILayoutDefinition>("Http requests can only be routed to pages and services. You must put the layout onto a page to route traffic to it.")
                .Invalid<IRegionDefinition>("Http requests can only be routed to pages and services.");

            Add<StyleAttribute>()
                .Valid<ILayoutDefinition>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>()
                .Invalid<IComponentDefinition>("Components do not render any Html elements that this style could be applied to. Specify the style within the Html that is rendered by the component.");

            Add<SuppliesDataAttribute>()
                .Valid<IDataProviderDefinition>();

            Add<UsesComponentAttribute>()
                .Valid<IRegionDefinition>()
                .Valid<IRegion>()
                .Invalid<ILayoutDefinition>("Please use the [RegionComponent] attribute instead so that the region can be specified.");

            Add<UsesLayoutAttribute>()
                .Valid<IPageDefinition>()
                .Valid<IRegionDefinition>()
                .Valid<IRegion>()
                .Invalid<ILayoutDefinition>("Please use the [RegionLayout] attribute instead so that the region can be specified.");

            Add<UsesRegionAttribute>()
                .Valid<ILayoutDefinition>()
                .Invalid<IPageDefinition>("Pages can only directly contain layouts. The layout defines the regions. Pages can override the regions of the layout using [RegionLayout] and [RegionComponent] attributes.");
        }

        private AttributeDefinition Add<T>()
        {
            var type = typeof(T);
            var definition = new AttributeDefinition(type);
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
            if (attributes.RegionComponents != null) CheckAttribute<T, RegionComponentAttribute>(result);
            if (attributes.RegionLayouts != null) CheckAttribute<T, RegionLayoutAttribute>(result);
            if (attributes.RenderHtmls != null) CheckAttribute<T, RenderHtmlAttribute>(result);
            if (attributes.Repeat != null) CheckAttribute<T, RepeatAttribute>(result);
            if (attributes.RequiresPermission != null) CheckAttribute<T, RequiresPermissionAttribute>(result);
            if (attributes.Routes != null) CheckAttribute<T, RouteAttribute>(result);
            if (attributes.Style != null) CheckAttribute<T, StyleAttribute>(result);
            if (attributes.SuppliesDatas != null) CheckAttribute<T, SuppliesDataAttribute>(result);
            if (attributes.UsesComponents != null) CheckAttribute<T, UsesComponentAttribute>(result);
            if (attributes.UsesLayouts != null) CheckAttribute<T, UsesLayoutAttribute>(result);
            if (attributes.UsesRegions != null) CheckAttribute<T, UsesRegionAttribute>(result);

            return result.Count > 0 ? result : null;
        }

        private void CheckAttribute<TElement, TAttribute>(List<string> result)
        {
            var definition = _attributeDefinitions[typeof(TAttribute)];
            var message = definition.Check<TElement>();
            if (!String.IsNullOrEmpty(message))
                result.Add(message);
        }
    }
}