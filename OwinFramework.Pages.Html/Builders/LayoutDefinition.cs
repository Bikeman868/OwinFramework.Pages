using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    public class LayoutDefinition : ILayoutDefinition
    {
        private readonly INameManager _nameManager;
        private readonly Layout _layout;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IRegionDependenciesFactory _regionDependenciesFactory;
        private readonly IComponentDependenciesFactory _componentDependenciesFactory;

        private ZoneSet _zoneSet;
        private readonly Dictionary<string, object> _regionElements;
        private readonly Dictionary<string, object> _regionLayouts;
        private readonly Dictionary<string, object> _regionComponents;

        private readonly List<FunctionDefinition> _functionDefinitions;
        private readonly List<CssDefinition> _cssDefinitions;

        private string _tag;
        private string[] _classNames;
        private string _style;

        private string _nestingTag = "div";
        private string[] _nestedClassNames;
        private string _nestedStyle;

        public LayoutDefinition(
            Layout layout,
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            IFluentBuilder fluentBuilder,
            IRegionDependenciesFactory regionDependenciesFactory,
            IComponentDependenciesFactory componentDependenciesFactory,
            IPackage package)
        {
            _layout = layout;
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _fluentBuilder = fluentBuilder;
            _regionDependenciesFactory = regionDependenciesFactory;
            _componentDependenciesFactory = componentDependenciesFactory;

            _regionElements = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _regionLayouts = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _regionComponents = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            _cssDefinitions = new List<CssDefinition>();
            _functionDefinitions = new List<FunctionDefinition>();

            if (package != null)
                _layout.Package = package;
        }

        ILayoutDefinition ILayoutDefinition.Name(string name)
        {
            if (string.IsNullOrEmpty(name)) return this;

            _layout.Name = name;

            return this;
        }

        ILayoutDefinition ILayoutDefinition.PartOf(IPackage package)
        {
            _layout.Package = package;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.PartOf(string packageName)
        {
            if (string.IsNullOrEmpty(packageName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolvePackageNames,
                (nm, l, n) => l.Package = nm.ResolvePackage(n),
                _layout,
                packageName);

            return this;
        }

        ILayoutDefinition ILayoutDefinition.ZoneNesting(string zoneNesting)
        {
            var position = 0;
            _zoneSet = ZoneSet.Parse(zoneNesting, ref position);
            return this;
        }

        ILayoutDefinition ILayoutDefinition.AssetDeployment(AssetDeployment assetDeployment)
        {
            _layout.AssetDeployment = assetDeployment;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.Region(string zoneName, IRegion region)
        {
            _regionElements[zoneName] = region;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.Region(string zoneName, string name)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new LayoutBuilderException("You must provide a region name when configuring layout zones");

            if (string.IsNullOrEmpty(name))
                throw new LayoutBuilderException("You must provide the name of the region element when configuring layout zones");

            _regionElements[zoneName] = name;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.Component(string zoneName, IComponent component)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new LayoutBuilderException("You must provide a region name when defining which component to place into a layout region");

            _regionComponents[zoneName] = component;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.Component(string zoneName, string componentName)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new LayoutBuilderException("You must provide a region name when defining which component to place into a layout region");

            if (string.IsNullOrEmpty(componentName))
                throw new LayoutBuilderException("You must provide the name of the component element when defining which component to place into a layout region");

            _regionComponents[zoneName] = componentName;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.Layout(string zoneName, ILayout layout)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new LayoutBuilderException("You must provide a region name when defining which layout to place into a layout region");

            _regionLayouts[zoneName] = layout;

            return this;
        }

        ILayoutDefinition ILayoutDefinition.Layout(string zoneName, string layoutName)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new LayoutBuilderException("You must provide a region name when defining which layout to place into a layout region");

            if (string.IsNullOrEmpty(zoneName))
                throw new LayoutBuilderException("You must provide the name of the layout element when defining which layout to place into a layout region");

            _regionLayouts[zoneName] = layoutName;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.Html(string zoneName, string textAssetName, string defaultHtml)
        {
            var htmlComponent = new HtmlComponent(_componentDependenciesFactory);
            htmlComponent.Html(textAssetName, defaultHtml);
            _regionComponents[zoneName] = htmlComponent;

            return this;
        }

        ILayoutDefinition ILayoutDefinition.Template(string zoneName, string templatePath)
        {
            var templateComponent = new TemplateComponent(_componentDependenciesFactory);
            templateComponent.BodyTemplate(templatePath);
            _regionComponents[zoneName] = templateComponent;

            return this;
        }

        ILayoutDefinition ILayoutDefinition.Tag(string tagName)
        {
            _tag = tagName;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.ClassNames(params string[] classNames)
        {
            _classNames = classNames;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.Style(string style)
        {
            _style = style;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.NestingTag(string tagName)
        {
            _nestingTag = tagName;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.NestedClassNames(params string[] classNames)
        {
            _nestedClassNames = classNames;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.NestedStyle(string style)
        {
            _nestedStyle = style;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.DeployCss(string cssSelector, string cssStyle)
        {
            if (string.IsNullOrEmpty(cssSelector))
                throw new LayoutBuilderException("No CSS selector is specified for component CSS asset");

            if (string.IsNullOrEmpty(cssStyle))
                throw new LayoutBuilderException("No CSS style is specified for component CSS asset");

            var cssDefinition = new CssDefinition
            {
                Selector = cssSelector.Trim(),
                Style = cssStyle.Trim()
            };

            if (!cssDefinition.Style.EndsWith(";"))
                cssDefinition.Style += ";";

            _cssDefinitions.Add(cssDefinition);

            return this;
        }

        ILayoutDefinition ILayoutDefinition.DeployFunction(string returnType, string functionName, string parameters, string functionBody, bool isPublic)
        {
            var functionDefinition = new FunctionDefinition
            {
                ReturnType = returnType,
                FunctionName = functionName,
                Parameters = parameters,
                Body = functionBody,
                IsPublic = isPublic
            };

            _functionDefinitions.Add(functionDefinition);

            return this;
        }

        ILayoutDefinition ILayoutDefinition.BindTo<T>(string scopeName)
        {
            var dataConsumer = _layout as IDataConsumer;
            if (dataConsumer == null)
                throw new LayoutBuilderException("This layout is not a consumer of data");

            dataConsumer.HasDependency<T>(scopeName);

            return this;
        }

        ILayoutDefinition ILayoutDefinition.BindTo(Type dataType, string scopeName)
        {
            if (dataType == null)
                throw new FluentBuilderException("To define data binding you must specify the type of data to bind");

            var dataConsumer = _layout as IDataConsumer;
            if (dataConsumer == null)
                throw new LayoutBuilderException("This layout is not a consumer of data");

            dataConsumer.HasDependency(dataType, scopeName);

            return this;
        }

        ILayoutDefinition ILayoutDefinition.DataProvider(string dataProviderName)
        {
            var dataConsumer = _layout as IDataConsumer;
            if (dataConsumer == null)
                throw new LayoutBuilderException("This layout is not a consumer of data");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, c, n) => c.HasDependency(nm.ResolveDataProvider(n)),
                dataConsumer,
                dataProviderName);

            return this;
        }

        ILayoutDefinition ILayoutDefinition.DataProvider(IDataProvider dataProvider)
        {
            var dataConsumer = _layout as IDataConsumer;
            if (dataConsumer == null)
                throw new LayoutBuilderException("This layout is not a consumer of data");

            dataConsumer.HasDependency(dataProvider);

            return this;
        }

        ILayoutDefinition ILayoutDefinition.DeployIn(IModule module)
        {
            _layout.Module = module;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.DeployIn(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, l, n) => l.Module = nm.ResolveModule(n),
                _layout,
                moduleName);

            return this;
        }

        ILayoutDefinition ILayoutDefinition.NeedsComponent(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
                throw new LayoutBuilderException("No component name provided in layout dependency");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, l, n) => l.NeedsComponent(nm.ResolveComponent(n, l.Package)),
                _layout,
                componentName);

            return this;
        }

        ILayoutDefinition ILayoutDefinition.NeedsComponent(IComponent component)
        {
            if (ReferenceEquals(component, null))
                throw new LayoutBuilderException("Null component reference for dependent component");

            _layout.NeedsComponent(component);

            return this;
        }

        ILayout ILayoutDefinition.Build()
        {
            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                nm =>
                {
                    var regionComponentKeys = _regionComponents.Keys.ToList();
                    foreach (var zoneName in regionComponentKeys)
                    {
                        var componentRef = _regionComponents[zoneName];
                        var componentName = componentRef as string;
                        var component = componentRef as IComponent;

                        if (componentName != null)
                            component = nm.ResolveComponent(componentName, _layout.Package);

                        _regionComponents[zoneName] = component;
                    }

                    var regionLayoutKeys = _regionLayouts.Keys.ToList();
                    foreach (var zoneName in regionLayoutKeys)
                    {
                        var layoutRef = _regionLayouts[zoneName];
                        var layoutName = layoutRef as string;
                        var layout = layoutRef as ILayout;

                        if (layoutName != null)
                            layout = nm.ResolveLayout(layoutName, _layout.Package);

                        _regionLayouts[zoneName] = layout;
                    }

                    ResolveZoneNames(_zoneSet, nm);

                    WriteOpeningTag();
                    WriteRegions(_zoneSet);
                    WriteClosingTag();
                });

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.CreateInstances, 
                () => SetRegionInstances(_zoneSet));

            _fluentBuilder.Register(_layout);
            return _layout;
        }

        #region Zone nesting

        private class ZoneSetRegion
        {
            public ZoneSet ChildZones;
            public string ZoneName;
            public string RegionElementName;
            public IRegion Region;
        }

        private class ZoneSet
        {
            public List<ZoneSetRegion> Zones;

            public static ZoneSet Parse(string zones, ref int position)
            {
                var result = new ZoneSet
                {
                    Zones = new List<ZoneSetRegion>()
                };

                var start = position;
                while (position < zones.Length)
                {
                    switch (zones[position])
                    {
                        case '(':
                            result.Zones.AddRange(BuildList(zones, start, position));
                            position++;
                            result.Zones.Add(new ZoneSetRegion { ChildZones = Parse(zones, ref position) });
                            position++;
                            start = position;
                            break;
                        case ')':
                            result.Zones.AddRange(BuildList(zones, start, position));
                            return result;
                        default:
                            position++;
                            break;
                    }
                }
                result.Zones.AddRange(BuildList(zones, start, position));
                return result;
            }

            private static IEnumerable<ZoneSetRegion> BuildList(string zones, int start, int end)
            {
                if (end <= start) return Enumerable.Empty<ZoneSetRegion>();

                var zoneNames = zones.Substring(start, end - start);
                return zoneNames
                    .Split(',')
                    .Select(s => s.Trim())
                    .Select(n => new ZoneSetRegion { ZoneName = n });
            }
        }

        private void ResolveZoneNames(ZoneSet zoneSet, INameManager nameManager)
        {
            if (zoneSet == null || zoneSet.Zones == null) return;

            foreach (var region in zoneSet.Zones)
            {
                if (region.Region == null)
                {
                    if (region.RegionElementName == null)
                    {
                        if (region.ZoneName != null)
                        {
                            object element;
                            if (_regionElements.TryGetValue(region.ZoneName, out element))
                            {
                                region.RegionElementName = element as string;
                                region.Region = element as IRegion;
                            }
                        }
                    }
                    if (region.RegionElementName != null && region.Region == null)
                    {
                        region.Region = nameManager.ResolveRegion(region.RegionElementName, _layout.Package);
                    }
                }

                if (region.Region == null && region.ZoneName != null)
                {
                    region.Region = new RegionComponent(_regionDependenciesFactory);
                }

                if (region.ZoneName != null)
                {
                    _layout.PopulateZone(region.ZoneName, region.Region);
                }

                if (region.ChildZones != null)
                {
                    ResolveZoneNames(region.ChildZones, nameManager);
                }
            }
        }

        private void WriteRegions(ZoneSet zoneSet)
        {
            if (zoneSet == null || zoneSet.Zones == null) return;

            foreach (var region in zoneSet.Zones)
            {
                if (region.ZoneName != null)
                {
                    _layout.AddZoneVisualElement(region.ZoneName);
                }

                if (region.ChildZones != null)
                {
                    WriteNestingOpeningTag();
                    WriteRegions(region.ChildZones);
                    WriteNestingClosingTag();
                }
            }
        }

        private void SetRegionInstances(ZoneSet zoneSet)
        {
            if (zoneSet == null || zoneSet.Zones == null) return;

            foreach (var region in zoneSet.Zones)
            {
                if (region.Region != null)
                {
                    IElement regionContent = null;

                    if (_regionComponents.ContainsKey(region.ZoneName))
                        regionContent = _regionComponents[region.ZoneName] as IElement;

                    if (_regionLayouts.ContainsKey(region.ZoneName))
                        regionContent = _regionLayouts[region.ZoneName] as IElement;

                    _layout.PopulateElement(region.ZoneName, regionContent);
                }

                if (region.ChildZones != null)
                {
                    SetRegionInstances(region.ChildZones);
                }
            }
        }

        #endregion

        private void WriteOpeningTag()
        {
            if (!string.IsNullOrEmpty(_tag))
            {
                var attributes = _htmlHelper.StyleAttributes(_style, _classNames, _layout.Package);
                _layout.AddVisualElement(w => { w.WriteOpenTag(_tag, attributes); w.WriteLine(); }, "layout container element");
            }
        }

        private void WriteClosingTag()
        {
            if (!string.IsNullOrEmpty(_tag))
                _layout.AddVisualElement(w => { w.WriteCloseTag(_tag); w.WriteLine(); }, null);
        }

        private void WriteNestingOpeningTag()
        {
            if (!string.IsNullOrEmpty(_nestingTag))
            {
                var attributes = _htmlHelper.StyleAttributes(_nestedStyle, _nestedClassNames, _layout.Package);
                _layout.AddVisualElement(w => { w.WriteOpenTag(_nestingTag, attributes); w.WriteLine(); }, "layout region grouping");
            }
        }

        private void WriteNestingClosingTag()
        {
            if (!string.IsNullOrEmpty(_nestingTag))
                _layout.AddVisualElement(w => { w.WriteCloseTag(_nestingTag); w.WriteLine(); }, null);
        }

    }
}
