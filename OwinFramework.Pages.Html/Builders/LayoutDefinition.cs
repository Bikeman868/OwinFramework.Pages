using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
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

        private RegionSet _regionSet;
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

        private class RegionSet
        {
            public List<object> Elements;

            public static RegionSet Parse(string regions, ref int position)
            {
                var result = new RegionSet
                {
                    Elements = new List<object>()
                };

                var start = position;
                while (position < regions.Length)
                {
                    switch (regions[position])
                    {
                        case '(':
                            result.Elements.AddRange(BuildList(regions, start, position));
                            position++;
                            result.Elements.Add(Parse(regions, ref position));
                            position++;
                            start = position;
                            break;
                        case ')':
                            result.Elements.AddRange(BuildList(regions, start, position));
                            return result;
                        default:
                            position++;
                            break;
                    }
                }
                result.Elements.AddRange(BuildList(regions, start, position));
                return result;
            }

            private static List<object> BuildList(string regions, int start, int end)
            {
                var result = new List<object>();
                if (end > start)
                {
                    var regionNames = regions.Substring(start, end - start);
                    result.AddRange(regionNames.Split(',').Select(s => s.Trim()));
                }
                return result;
            }
        }

        public LayoutDefinition(
            Layout layout,
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            IFluentBuilder fluentBuilder,
            IPackage package)
        {
            _layout = layout;
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _fluentBuilder = fluentBuilder;

            _regionElements = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _regionLayouts = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _regionComponents = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            _cssDefinitions = new List<CssDefinition>();
            _functionDefinitions = new List<FunctionDefinition>();

            if (package != null)
                _layout.Package = package;
        }

        public ILayoutDefinition Name(string name)
        {
            if (string.IsNullOrEmpty(name)) return this;

            _layout.Name = name;

            return this;
        }

        public ILayoutDefinition PartOf(IPackage package)
        {
            _layout.Package = package;
            return this;
        }

        public ILayoutDefinition PartOf(string packageName)
        {
            if (string.IsNullOrEmpty(packageName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolvePackageNames,
                (nm, l, n) => l.Package = nm.ResolvePackage(n),
                _layout,
                packageName);

            return this;
        }

        public ILayoutDefinition RegionNesting(string regionNesting)
        {
            var position = 0;
            _regionSet = RegionSet.Parse(regionNesting, ref position);
            return this;
        }

        public ILayoutDefinition AssetDeployment(AssetDeployment assetDeployment)
        {
            _layout.AssetDeployment = assetDeployment;
            return this;
        }

        public ILayoutDefinition Region(string regionName, IRegion region)
        {
            _regionElements[regionName] = region;
            return this;
        }

        public ILayoutDefinition Region(string regionName, string name)
        {
            if (string.IsNullOrEmpty(regionName))
                throw new LayoutBuilderException("You must provide a region name when configuring layout regions");

            if (string.IsNullOrEmpty(name))
                throw new LayoutBuilderException("You must provide the name of the region element when configuring layout regions");

            _regionElements[regionName] = name;
            return this;
        }

        public ILayoutDefinition Component(string regionName, IComponent component)
        {
            if (string.IsNullOrEmpty(regionName))
                throw new LayoutBuilderException("You must provide a region name when defining which component to place into a layout region");

            _regionComponents[regionName] = component;
            return this;
        }

        public ILayoutDefinition Component(string regionName, string componentName)
        {
            if (string.IsNullOrEmpty(regionName))
                throw new LayoutBuilderException("You must provide a region name when defining which component to place into a layout region");

            if (string.IsNullOrEmpty(componentName))
                throw new LayoutBuilderException("You must provide the name of the component element when defining which component to place into a layout region");

            _regionComponents[regionName] = componentName;
            return this;
        }

        public ILayoutDefinition Layout(string regionName, ILayout layout)
        {
            if (string.IsNullOrEmpty(regionName))
                throw new LayoutBuilderException("You must provide a region name when defining which layout to place into a layout region");

            _regionLayouts[regionName] = layout;

            return this;
        }

        public ILayoutDefinition Layout(string regionName, string layoutName)
        {
            if (string.IsNullOrEmpty(regionName))
                throw new LayoutBuilderException("You must provide a region name when defining which layout to place into a layout region");

            if (string.IsNullOrEmpty(regionName))
                throw new LayoutBuilderException("You must provide the name of the layout element when defining which layout to place into a layout region");

            _regionLayouts[regionName] = layoutName;
            return this;
        }

        public ILayoutDefinition Tag(string tagName)
        {
            _tag = tagName;
            return this;
        }

        public ILayoutDefinition ClassNames(params string[] classNames)
        {
            _classNames = classNames;
            return this;
        }

        public ILayoutDefinition Style(string style)
        {
            _style = style;
            return this;
        }

        public ILayoutDefinition NestingTag(string tagName)
        {
            _nestingTag = tagName;
            return this;
        }

        public ILayoutDefinition NestedClassNames(params string[] classNames)
        {
            _nestedClassNames = classNames;
            return this;
        }

        public ILayoutDefinition NestedStyle(string style)
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
                (nm, l, n) => l.NeedsComponent(nm.ResolveComponent(n)),
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

        public ILayout Build()
        {
            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm) =>
                {
                    var regionComponentKeys = _regionComponents.Keys.ToList();
                    foreach (var regionName in regionComponentKeys)
                    {
                        var componentRef = _regionComponents[regionName];
                        var componentName = componentRef as string;
                        var component = componentRef as IComponent;

                        if (componentName != null)
                            component = nm.ResolveComponent(componentName, _layout.Package);

                        _regionComponents[regionName] = component;
                    }

                    var regionLayoutKeys = _regionLayouts.Keys.ToList();
                    foreach (var regionName in regionLayoutKeys)
                    {
                        var layoutRef = _regionLayouts[regionName];
                        var layoutName = layoutRef as string;
                        var layout = layoutRef as ILayout;

                        if (layoutName != null)
                            layout = nm.ResolveLayout(layoutName, _layout.Package);

                        _regionLayouts[regionName] = layout;
                    }

                    WriteOpeningTag();
                    AddRegionSet(_regionSet);
                    WriteClosingTag();
                });

            _fluentBuilder.Register(_layout);
            return _layout;
        }

        private void AddRegionSet(RegionSet regionSet)
        {
            if (regionSet == null || regionSet.Elements == null) return;

            foreach (var element in regionSet.Elements)
            {
                var regionName = element as string;
                var childRegionSet = element as RegionSet;

                if (regionName != null)
                {
                    if (!_regionElements.ContainsKey(regionName))
                    {
                        _layout.AddVisualElement(w => w.WriteElement("p", "There is no region instance in the '" + regionName + "' region"), null);
                        continue;
                    }

                    var regionElement = _regionElements[regionName];
                    var region = regionElement as IRegion;
                    var regionElementName = regionElement as string;

                    if (region == null && regionElementName != null)
                    {
                        region = _nameManager.ResolveRegion(regionElementName, _layout.Package);
                        if (region == null)
                            _layout.AddVisualElement(w => w.WriteElement("p", "Unknown region element '" + regionElementName + "'"), null);
                    }

                    if (region != null)
                    {
                        if (_regionComponents.ContainsKey(regionName))
                        {
                            _layout.AddRegion(regionName, region, (IComponent)_regionComponents[regionName]);
                        }
                        else if (_regionLayouts.ContainsKey(regionName))
                        {
                            _layout.AddRegion(regionName, region, ((ILayout)_regionLayouts[regionName]).CreateInstance());
                        }
                        else
                            _layout.AddRegion(regionName, region);
                    }
                }
                else if (childRegionSet != null)
                {
                    WriteNestingOpeningTag();
                    AddRegionSet(childRegionSet);
                    WriteNestingClosingTag();
                }
            }
        }

        private void WriteOpeningTag()
        {
            if (!string.IsNullOrEmpty(_tag))
            {
                var attributes = _htmlHelper.StyleAttributes(_style, _classNames, _layout.Package);
                _layout.AddVisualElement(w => w.WriteOpenTag(_tag, attributes), "layout container element");
            }
        }

        private void WriteClosingTag()
        {
            if (!string.IsNullOrEmpty(_tag))
                _layout.AddVisualElement(w => w.WriteCloseTag(_tag), null);
        }

        private void WriteNestingOpeningTag()
        {
            if (!string.IsNullOrEmpty(_nestingTag))
            {
                var attributes = _htmlHelper.StyleAttributes(_nestedStyle, _nestedClassNames, _layout.Package);
                _layout.AddVisualElement(w => w.WriteOpenTag(_nestingTag, attributes), "grouping regions in layout");
            }
        }

        private void WriteNestingClosingTag()
        {
            if (!string.IsNullOrEmpty(_nestingTag))
                _layout.AddVisualElement(w => w.WriteCloseTag(_nestingTag), null);
        }
    }
}
